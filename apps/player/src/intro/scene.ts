// Scroll-driven 3D scene behind the intro page (pages/IntroPage.vue). One renderer, four stages:
//   0 court — a floodlit hard court and a served ball      1 map — GPS radius rings and venue pins
//   2 slots — the booking time-slot matrix                  3 pay/play — particles: a PromptPay-style QR, then a tennis ball
// Five chapters scroll past it (the last two share stage 3). Everything is procedural: no model or image files.
// Colours come from the --bkk-* tokens of @bkk/ui, read from the page root.
import {
  AdditiveBlending, BackSide, BoxGeometry, BufferAttribute, BufferGeometry, CanvasTexture, CatmullRomCurve3, CircleGeometry,
  Color, ConeGeometry, CylinderGeometry, DirectionalLight, DoubleSide, DynamicDrawUsage, ExtrudeGeometry, Fog, Group,
  HemisphereLight, InstancedMesh, Mesh, MeshBasicMaterial, MeshStandardMaterial, Object3D, PerspectiveCamera, PlaneGeometry,
  PMREMGenerator, PointLight, Points, RepeatWrapping, RingGeometry, Scene, ShaderMaterial, Shape, SphereGeometry, Sprite,
  SpriteMaterial, SRGBColorSpace, TorusGeometry, Vector3, WebGLRenderer,
  type Material, type Texture, type WebGLRenderTarget,
} from 'three';
import QRCode from 'qrcode';

/** What the page needs to know each frame; `onTick` only fires when one of these changes. */
export interface IntroTick {
  chapter: number;
  /** whether the chapter's text panel should be visible (hidden while the scene cross-fades) */
  shown: boolean;
  /** 0..1 over the whole page */
  progress: number;
  /** GPS radius reached on the map stage: 0 (still locating), 3, 5 or 10 */
  radiusKm: number;
  /** slots selected on the matrix stage: 0..2 */
  picked: number;
  /** seconds left of the 10-minute hold */
  holdSec: number;
  /** 0..100: how much of the QR has formed */
  qr: number;
}

export interface IntroOptions {
  /** page root: tokens are read from it, and `--intro-night` (the fog colour) is set on it */
  root: HTMLElement;
  canvas: HTMLCanvasElement;
  veil: HTMLElement;
  /** the five tall chapter sections, in order */
  sections: HTMLElement[];
  /** labels for the 3 / 5 / 10 km rings, positioned every frame */
  tags: HTMLElement[];
  onTick: (s: IntroTick) => void;
}

export interface IntroScene {
  goTo(chapter: number): void;
  destroy(): void;
}

type V3 = readonly [number, number, number];
interface Cam { p0: V3; p1: V3; t0: V3; t1: V3; sx: number; sy: number; fog: readonly [number, number] }

const TAU = Math.PI * 2;
const clamp = (x: number, a = 0, b = 1) => Math.min(b, Math.max(a, x));
const lerp = (a: number, b: number, k: number) => a + (b - a) * k;
const ease = (k: number) => k * k * (3 - 2 * k);
const sstep = (a: number, b: number, x: number) => ease(clamp((x - a) / (b - a)));
const rng = (seed: number) => () => {
  seed = (seed + 0x6d2b79f5) | 0;
  let x = Math.imul(seed ^ (seed >>> 15), 1 | seed);
  x = (x + Math.imul(x ^ (x >>> 7), 61 | x)) ^ x;
  return ((x ^ (x >>> 14)) >>> 0) / 4294967296;
};

const STAGE = [0, 1, 2, 3, 3] as const;
const LAST = STAGE.length - 1;
/** portion of each chapter during which its text panel shows */
const SHOW: readonly (readonly [number, number])[] = [[-1, 0.8], [0.14, 0.84], [0.14, 0.84], [0.12, 0.9], [0.1, 2]];
export const RADII_KM = [3, 5, 10] as const;
export const COURT_LENGTH_M = 23.77;

/** Returns null when WebGL is unavailable; the page then stays a plain stacked document. */
export function createIntroScene(o: IntroOptions): IntroScene | null {
  // Ask for the context ourselves: three logs console errors when it cannot get one, and "no WebGL" is an expected case here.
  const gl = o.canvas.getContext('webgl2', { alpha: false, antialias: true, powerPreference: 'high-performance' });
  if (!gl) return null;
  let renderer: WebGLRenderer;
  try {
    renderer = new WebGLRenderer({ canvas: o.canvas, context: gl, antialias: true });
  } catch {
    return null;
  }

  const reduced = matchMedia('(prefers-reduced-motion: reduce)').matches;
  const coarse = matchMedia('(pointer: coarse)').matches;

  /* ---------- tokens ---------- */
  const css = getComputedStyle(o.root);
  const tok = (name: string, fallback: string) => css.getPropertyValue(name).trim() || fallback;
  const T = {
    forest: tok('--bkk-forest', '#064e3b'), forestDeep: tok('--bkk-forest-deep', '#04392b'), forestInk: tok('--bkk-forest-ink', '#003527'),
    volt: tok('--bkk-volt', '#84cc16'), voltBright: tok('--bkk-volt-bright', '#a3e635'), clay: tok('--bkk-clay', '#ea580c'),
    slate: tok('--bkk-slate', '#0f172a'), surface: tok('--bkk-surface', '#ffffff'), surface3: tok('--bkk-surface-3', '#f1f5f9'),
    line: tok('--bkk-line-strong', '#cbd5e1'), text3: tok('--bkk-text-3', '#94a3b8'), mint: tok('--bkk-mint', '#ecfdf5'),
  };
  const col = (hex: string) => new Color(hex);
  const night = col(T.forestInk).lerp(col(T.slate), 0.4).multiplyScalar(0.72);
  o.root.style.setProperty('--intro-night', `#${night.getHexString()}`);

  /* ---------- scene, lights, shared materials ---------- */
  const scene = new Scene();
  const fog = new Fog(night, 9, 60);
  scene.fog = fog;
  renderer.setClearColor(night, 1);
  const camera = new PerspectiveCamera(38, 1, 0.1, 260);
  scene.add(new HemisphereLight(col(T.mint).multiplyScalar(0.55), night, 0.7));
  const moon = new DirectionalLight(0xcfe6ff, 0.55);
  moon.position.set(-6, 9, -4);
  scene.add(moon);

  const textures: Texture[] = [];
  const canvasTex = (w: number, h: number, draw: (g: CanvasRenderingContext2D) => void) => {
    const c = document.createElement('canvas');
    c.width = w; c.height = h;
    const g = c.getContext('2d');
    if (g) draw(g);
    const t = new CanvasTexture(c);
    t.colorSpace = SRGBColorSpace;
    textures.push(t);
    return t;
  };
  const glowTex = canvasTex(128, 128, (g) => {
    const gr = g.createRadialGradient(64, 64, 0, 64, 64, 64);
    gr.addColorStop(0, 'rgba(255,255,255,1)'); gr.addColorStop(0.22, 'rgba(255,255,255,.6)');
    gr.addColorStop(0.55, 'rgba(255,255,255,.14)'); gr.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = gr; g.fillRect(0, 0, 128, 128);
  });
  const halo = (size: number, color: Color | string, opacity = 0.9) => {
    const s = new Sprite(new SpriteMaterial({ map: glowTex, color, transparent: true, depthWrite: false, opacity }));
    s.scale.set(size, size, 1);
    return s;
  };
  const flat = <M extends Object3D>(m: M) => { m.rotation.x = -Math.PI / 2; return m; };
  const dummy = new Object3D();

  const M = {
    ground: new MeshStandardMaterial({ color: night.clone().multiplyScalar(2.2), roughness: 1 }),
    apron: new MeshStandardMaterial({ color: col(T.clay).multiplyScalar(0.5), roughness: 0.95 }),
    court: new MeshStandardMaterial({ color: col(T.forest).lerp(col(T.volt), 0.26), roughness: 0.9 }),
    line: new MeshStandardMaterial({ color: T.surface, emissive: T.surface, emissiveIntensity: 0.35, roughness: 0.8 }),
    metal: new MeshStandardMaterial({ color: col(T.slate).multiplyScalar(1.6), roughness: 0.5, metalness: 0.5 }),
    lamp: new MeshBasicMaterial({ color: T.surface }),
    beam: new MeshBasicMaterial({ color: col(T.mint), transparent: true, opacity: 0.032, blending: AdditiveBlending, side: DoubleSide, depthWrite: false, fog: false }),
    ball: new MeshStandardMaterial({ color: T.voltBright, emissive: T.volt, emissiveIntensity: 0.55, roughness: 0.75 }),
    shadow: new MeshBasicMaterial({ map: glowTex, color: 0x000000, transparent: true, opacity: 0.5, depthWrite: false }),
    block: new MeshStandardMaterial({ color: col(T.forestDeep).multiplyScalar(1.5), roughness: 0.85 }),
    river: new MeshStandardMaterial({ color: col(T.slate).multiplyScalar(2.2), roughness: 0.35, metalness: 0.2 }),
    pinDim: new MeshStandardMaterial({ color: T.text3, roughness: 0.6 }),
    pinLit: new MeshStandardMaterial({ color: T.voltBright, emissive: T.volt, emissiveIntensity: 0.9, roughness: 0.5 }),
    plat: new MeshStandardMaterial({ color: col(T.forestDeep).multiplyScalar(1.7), roughness: 0.7 }),
    volt: new MeshBasicMaterial({ color: T.voltBright }),
  };

  const stages = [0, 1, 2, 3].map(() => { const g = new Group(); g.visible = false; scene.add(g); return g; });
  const groundDisc = () => flat(new Mesh(new CircleGeometry(90, 48), M.ground));

  /* ---------- stage 0: the floodlit court (true proportions, 1 m = 0.4 units) ---------- */
  const S = 0.4;
  const CL = COURT_LENGTH_M * S, CW = 10.97 * S, SW = 8.23 * S, SVC = 6.4 * S;
  const court = (() => {
    const g = stages[0]!;
    g.add(groundDisc());
    const slab = (w: number, l: number, m: Material, y: number, x = 0, z = 0) => {
      const p = flat(new Mesh(new PlaneGeometry(w, l), m));
      p.position.set(x, y, z);
      g.add(p);
    };
    slab(18.29 * S, 36.57 * S, M.apron, 0.004);
    slab(CW, CL, M.court, 0.008);
    const lw = 0.035, y = 0.012;
    for (const sx of [-1, 1]) {
      slab(lw, CL, M.line, y, sx * (CW / 2 - lw / 2));                 // doubles sidelines
      slab(lw, CL, M.line, y, sx * (SW / 2 - lw / 2));                 // singles sidelines
      slab(CW, lw * 1.6, M.line, y, 0, sx * (CL / 2 - lw * 0.8));      // baselines
      slab(SW, lw, M.line, y, 0, sx * SVC);                            // service lines
      slab(lw, 0.12, M.line, y, 0, sx * (CL / 2 - 0.06 - lw * 1.6));   // centre marks
    }
    slab(lw, SVC * 2, M.line, y);                                      // centre service line

    const netW = CW + 1.83 * S, netH = 1.0 * S;
    const netTex = canvasTex(64, 64, (c) => {
      c.strokeStyle = 'rgba(255,255,255,.5)'; c.lineWidth = 2;
      for (let i = 0; i <= 64; i += 16) { c.beginPath(); c.moveTo(i, 0); c.lineTo(i, 64); c.moveTo(0, i); c.lineTo(64, i); c.stroke(); }
    });
    netTex.wrapS = netTex.wrapT = RepeatWrapping;
    netTex.repeat.set(44, 4);
    const net = new Mesh(new PlaneGeometry(netW, netH), new MeshBasicMaterial({ map: netTex, transparent: true, side: DoubleSide, depthWrite: false }));
    net.position.y = netH / 2;
    g.add(net);
    const tape = new Mesh(new BoxGeometry(netW, 0.035, 0.02), M.line);
    tape.position.y = netH;
    g.add(tape);
    for (const sx of [-1, 1]) {
      const post = new Mesh(new CylinderGeometry(0.022, 0.022, netH + 0.04, 8), M.metal);
      post.position.set(sx * netW / 2, (netH + 0.04) / 2, 0);
      g.add(post);
    }

    const up = new Vector3(0, 1, 0), dir = new Vector3();
    const lampTint = col(T.mint);
    for (const [mx, mz] of [[-4.1, -3.3], [4.1, -3.3], [-4.1, 3.3], [4.1, 3.3]] as const) {
      const light = new PointLight(lampTint, 42, 20, 2);
      light.position.set(mx * 0.9, 4.5, mz * 0.9);
      g.add(light);
      if (mx > 0 && mz > 0) continue; // the mast nearest the camera would stand in front of the court: keep its light only
      const pole = new Mesh(new CylinderGeometry(0.03, 0.045, 4.6, 8), M.metal);
      pole.position.set(mx, 2.3, mz);
      g.add(pole);
      const head = new Mesh(new BoxGeometry(0.62, 0.2, 0.1), M.lamp);
      head.position.set(mx, 4.62, mz);
      head.lookAt(mx * 0.2, 0, mz * 0.3);
      g.add(head);
      const glow = halo(2.6, lampTint, 0.85);
      glow.position.set(mx * 0.98, 4.6, mz * 0.98);
      g.add(glow);
      // the visible beam: an open cone from the lamp down to the court
      const apex = new Vector3(mx, 4.6, mz), base = new Vector3(mx * 0.3, 0, mz * 0.4);
      const len = apex.distanceTo(base);
      const beam = new Mesh(new ConeGeometry(2.4, len, 28, 1, true), M.beam);
      beam.position.copy(apex).add(base).multiplyScalar(0.5);
      beam.quaternion.setFromUnitVectors(up, dir.subVectors(apex, base).normalize());
      g.add(beam);
    }

    const ball = new Mesh(new SphereGeometry(0.085, 24, 16), M.ball);
    const glow = halo(0.6, T.voltBright, 0.7);
    const shadow = flat(new Mesh(new PlaneGeometry(0.5, 0.5), M.shadow));
    shadow.position.y = 0.018;
    g.add(ball, glow, shadow);
    return { ball, glow, shadow };
  })();

  /* ---------- stage 1: the map — GPS radius rings and venue pins ---------- */
  const KM = 0.5; // scene units per km
  const map = (() => {
    const g = stages[1]!;
    g.add(groundDisc());
    const city = new Group();
    city.rotation.y = 0.21;
    g.add(city);

    // a river bend on the west side
    const bend: V3[] = [[-11, 0, -8], [-7.6, 0, -3.6], [-5.7, 0, -0.4], [-6.9, 0, 2.6], [-4.7, 0, 5.6], [-5.6, 0, 10]];
    const pts = new CatmullRomCurve3(bend.map((p) => new Vector3(p[0], p[1], p[2]))).getPoints(64);
    const rp: number[] = [], ri: number[] = [];
    pts.forEach((p, i) => {
      const a = pts[Math.max(0, i - 1)]!, b = pts[Math.min(pts.length - 1, i + 1)]!;
      const tx = b.x - a.x, tz = b.z - a.z, tl = Math.hypot(tx, tz) || 1, nx = -tz / tl, nz = tx / tl;
      rp.push(p.x + nx * 0.46, 0.012, p.z + nz * 0.46, p.x - nx * 0.46, 0.012, p.z - nz * 0.46);
      if (i) { const k = i * 2; ri.push(k - 2, k, k - 1, k - 1, k, k + 1); }
    });
    const rg = new BufferGeometry();
    rg.setAttribute('position', new BufferAttribute(new Float32Array(rp), 3));
    rg.setIndex(ri);
    rg.computeVertexNormals();
    const river = new Mesh(rg, M.river);
    (river.material as MeshStandardMaterial).side = DoubleSide;
    city.add(river);
    const nearRiver = (x: number, z: number) => pts.some((p) => (p.x - x) ** 2 + (p.z - z) ** 2 < 0.55);

    // city blocks, with avenues left open
    const r = rng(41);
    const cells: { x: number; z: number; h: number; w: number; d: number }[] = [];
    for (let i = -17; i <= 17; i++) for (let j = -17; j <= 17; j++) {
      const x = i * 0.62 + (r() - 0.5) * 0.08, z = j * 0.62 + (r() - 0.5) * 0.08;
      if (i % 6 === 0 || j % 7 === 0 || r() < 0.12 || Math.hypot(x, z) < 0.5 || nearRiver(x, z)) continue;
      cells.push({ x, z, h: 0.04 + r() ** 2.2 * 0.34, w: 0.36 + r() * 0.16, d: 0.36 + r() * 0.16 });
    }
    const blocks = new InstancedMesh(new BoxGeometry(1, 1, 1), M.block, cells.length);
    cells.forEach((c, i) => {
      dummy.position.set(c.x, c.h / 2, c.z);
      dummy.rotation.set(0, 0, 0);
      dummy.scale.set(c.w, c.h, c.d);
      dummy.updateMatrix();
      blocks.setMatrixAt(i, dummy.matrix);
    });
    city.add(blocks);

    // HUD-like overlays drawn over the city
    const overlay = (geo: BufferGeometry, opacity: number) => {
      const m = flat(new Mesh(geo, new MeshBasicMaterial({ color: T.voltBright, transparent: true, opacity, depthTest: false, side: DoubleSide, fog: false })));
      m.position.y = 0.42;
      m.renderOrder = 5;
      g.add(m);
      return m;
    };
    const rings = RADII_KM.map((km) => overlay(new RingGeometry(km * KM - 0.012, km * KM + 0.012, 160), 0));
    const sweep = overlay(new RingGeometry(0.985, 1.015, 160), 0);
    const area = overlay(new CircleGeometry(1, 96), 0);

    const you = new Mesh(new SphereGeometry(0.09, 20, 14), M.volt);
    you.position.y = 0.42;
    const youGlow = halo(1.1, T.voltBright, 0.85);
    youGlow.position.y = 0.42;
    g.add(you, youGlow);
    const lamp = new PointLight(col(T.voltBright), 6, 9, 2);
    lamp.position.y = 1.4;
    g.add(lamp);

    // venues: [distance km, bearing rad]
    const VENUES: readonly (readonly [number, number])[] = [[1.8, 0.6], [2.6, 2.5], [2.9, 4.5], [3.9, 1.5], [4.6, 3.5], [6.3, 5.4], [7.4, 0.2], [8.6, 2.2], [9.5, 4.0], [12.4, 1.1]];
    const stemGeo = new CylinderGeometry(0.012, 0.012, 0.56, 6), headGeo = new SphereGeometry(0.1, 18, 12);
    const pins = VENUES.map(([km, a]) => {
      const pin = new Group();
      pin.position.set(Math.cos(a) * km * KM, 0, Math.sin(a) * km * KM);
      const stem = new Mesh(stemGeo, M.pinDim);
      stem.position.y = 0.28;
      const head = new Mesh(headGeo, M.pinDim);
      head.position.y = 0.62;
      const glow = halo(0.7, T.voltBright, 0);
      glow.position.y = 0.62;
      pin.add(stem, head, glow);
      g.add(pin);
      return { km, head, stem, glow, k: 0 };
    });
    return { rings, sweep, area, youGlow, pins };
  })();

  /* ---------- stage 2: the time-slot matrix (the booking engine's own tiles, in 3D) ---------- */
  const slots = (() => {
    const g = stages[2]!;
    const grid = new Group();
    g.add(grid);
    const TW = 1.5, TH = 0.8, HW = 0.8, GAP = 0.16, DEPTH = 0.1;
    const HOURS = [16, 17, 18, 19, 20, 21], COURTS = 3;
    const TAKEN = new Set(['0-1', '0-2', '0-5', '1-4', '2-0', '2-3']); // court-column
    const PICK: readonly (readonly [number, number])[] = [[1, 1], [1, 2]];   // court 2 at 17:00 and 18:00

    const rounded = (w: number, h: number, rad: number, depth: number) => {
      const s = new Shape(), x = -w / 2, y = -h / 2;
      s.moveTo(x + rad, y); s.lineTo(x + w - rad, y); s.quadraticCurveTo(x + w, y, x + w, y + rad);
      s.lineTo(x + w, y + h - rad); s.quadraticCurveTo(x + w, y + h, x + w - rad, y + h);
      s.lineTo(x + rad, y + h); s.quadraticCurveTo(x, y + h, x, y + h - rad);
      s.lineTo(x, y + rad); s.quadraticCurveTo(x, y, x + rad, y);
      const geo = new ExtrudeGeometry(s, { depth, bevelEnabled: false, curveSegments: 6 });
      geo.translate(0, 0, -depth / 2);
      return geo;
    };
    const tileGeo = rounded(TW, TH, 0.13, DEPTH), headGeo = rounded(HW, TH, 0.13, DEPTH), rimGeo = rounded(TW + 0.1, TH + 0.1, 0.17, 0.04);

    // Tile faces are painted on canvases with the product's own type; repainted once the web fonts have loaded.
    const DISPLAY = '"Space Grotesk", system-ui, sans-serif';
    type Kind = 'free' | 'taken' | 'picked' | 'head';
    const painters: (() => void)[] = [];
    const face = (w: number, kind: Kind, label: string) => {
      const pw = Math.round(256 * w), ph = Math.round(256 * TH);
      const canvas = document.createElement('canvas');
      canvas.width = pw; canvas.height = ph;
      const c = canvas.getContext('2d');
      const tex = new CanvasTexture(canvas);
      tex.colorSpace = SRGBColorSpace;
      tex.repeat.set(1 / w, 1 / TH);   // extruded faces carry shape coordinates as UVs
      tex.offset.set(0.5, 0.5);
      textures.push(tex);
      const paint = () => {
        if (!c) return;
        c.fillStyle = kind === 'picked' ? T.forest : kind === 'head' ? T.forestDeep : kind === 'taken' ? T.surface3 : T.surface;
        c.fillRect(0, 0, pw, ph);
        c.textBaseline = 'middle';
        if (kind === 'head') {
          c.textAlign = 'center';
          c.fillStyle = T.voltBright; c.font = `600 26px ${DISPLAY}`;
          c.fillText('COURT', pw / 2, ph * 0.26);
          c.fillStyle = T.surface; c.font = `700 104px ${DISPLAY}`;
          c.fillText(label, pw / 2, ph * 0.63);
        } else {
          c.textAlign = 'left';
          c.fillStyle = kind === 'picked' ? T.surface : kind === 'taken' ? T.text3 : T.slate;
          c.font = `700 92px ${DISPLAY}`;
          c.fillText(label, 34, ph / 2 + 4);
          const cx = pw - 62, cy = ph / 2;
          if (kind === 'taken') {
            c.strokeStyle = T.text3; c.lineWidth = 7;
            c.beginPath(); c.moveTo(28, cy + 4); c.lineTo(34 + c.measureText(label).width + 8, cy + 4); c.stroke();
          } else if (kind === 'picked') {
            c.fillStyle = T.voltBright; c.beginPath(); c.arc(cx, cy, 34, 0, TAU); c.fill();
            c.strokeStyle = T.forestInk; c.lineWidth = 9; c.lineCap = 'round'; c.lineJoin = 'round';
            c.beginPath(); c.moveTo(cx - 15, cy + 1); c.lineTo(cx - 4, cy + 12); c.lineTo(cx + 16, cy - 11); c.stroke();
          } else {
            c.strokeStyle = T.line; c.lineWidth = 6; c.beginPath(); c.arc(cx, cy, 30, 0, TAU); c.stroke();
          }
        }
        tex.needsUpdate = true;
      };
      painters.push(paint);
      paint();
      return new MeshBasicMaterial({ map: tex });
    };
    const side = { free: new MeshBasicMaterial({ color: T.line }), taken: new MeshBasicMaterial({ color: T.text3 }), picked: new MeshBasicMaterial({ color: T.forestDeep }) };

    const hh = (h: number) => `${h}:00`;
    const tiles: { mesh: Mesh; court: number; column: number }[] = [];
    const heads: Mesh[] = [];
    for (let c = 0; c < COURTS; c++) {
      const head = new Mesh(headGeo, [face(HW, 'head', String(c + 1)), side.picked]);
      heads.push(head);
      grid.add(head);
      HOURS.forEach((h, column) => {
        const kind: Kind = TAKEN.has(`${c}-${column}`) ? 'taken' : 'free';
        const mesh = new Mesh(tileGeo, [face(TW, kind, hh(h)), kind === 'taken' ? side.taken : side.free]);
        tiles.push({ mesh, court: c, column });
        grid.add(mesh);
      });
    }
    const picks = PICK.map(([c, column]) => {
      const tile = tiles.find((x) => x.court === c && x.column === column)!;
      const rim = new Mesh(rimGeo, M.volt);
      rim.visible = false;
      const glow = halo(3.2, T.voltBright, 0);
      grid.add(rim, glow);
      const free = tile.mesh.material as Material[];
      const picked = [face(TW, 'picked', hh(HOURS[column]!)), side.picked];
      return { tile, rim, glow, free, picked, on: false, k: 0 };
    });

    /** camera distance that fits the grid's width, and how far below the grid the camera aims (keeps it above the text) */
    const view = { dist: 9, drop: 1 };
    /** Landscape: courts are rows and hours run across. Portrait: hours run down, as on a phone (first four only). */
    const layout = (aspect: number) => {
      const portrait = aspect < 1, stepX = TW + GAP, stepY = TH + GAP;
      const width = portrait ? COURTS * TW + (COURTS - 1) * GAP : HW + GAP + HOURS.length * TW + (HOURS.length - 1) * GAP;
      grid.rotation.y = portrait ? 0 : 0.12;
      if (portrait) {
        heads.forEach((h, c) => h.position.set((c - 1) * stepX, 2 * stepY, 0));
        tiles.forEach(({ mesh, court: c, column }) => { mesh.visible = column < 4; mesh.position.set((c - 1) * stepX, (1 - column) * stepY, 0); });
      } else {
        const left = -width / 2;
        heads.forEach((h, c) => h.position.set(left + HW / 2, (1 - c) * stepY, 0));
        tiles.forEach(({ mesh, court: c, column }) => { mesh.visible = true; mesh.position.set(left + HW + GAP + TW / 2 + column * stepX, (1 - c) * stepY, 0); });
      }
      picks.forEach((p) => { p.rim.position.copy(p.tile.mesh.position); p.glow.position.copy(p.tile.mesh.position); });
      const tan = Math.tan((camera.fov * Math.PI) / 360);
      view.dist = Math.max(7, width / 0.9 / (2 * tan * aspect));
      view.drop = 2 * view.dist * tan * (portrait ? 0.2 : 0.17);
    };
    const repaint = () => painters.forEach((p) => p());
    return { picks, layout, view, repaint };
  })();

  /* ---------- stage 3: particles — dust on the platform, a QR, then a ball ---------- */
  const N = coarse ? 7000 : 14000;
  const dust = (() => {
    const g = stages[3]!;
    g.add(groundDisc());
    for (const [a, b, h, y] of [[4.6, 4.75, 0.14, 0.07], [3.1, 3.2, 0.14, 0.21], [1.75, 1.82, 0.16, 0.36]] as const) {
      const m = new Mesh(new CylinderGeometry(a, b, h, 96), M.plat);
      m.position.y = y;
      g.add(m);
    }
    for (const [rad, y] of [[3.12, 0.285], [1.77, 0.445], [4.62, 0.145]] as const) {
      const m = flat(new Mesh(new TorusGeometry(rad, 0.013, 6, 160), M.volt));
      m.position.y = y;
      g.add(m);
    }
    const key = new PointLight(col(T.mint), 26, 16, 2);
    key.position.set(-2.4, 5.2, 3.2);
    g.add(key);

    const pr = rng(23);
    const mk = () => ({ p: new Float32Array(N * 3), n: new Float32Array(N * 3), c: new Float32Array(N * 3) });
    const A = mk(), B = mk(), C = mk(); // scatter, QR, ball
    const put = (t: ReturnType<typeof mk>, i: number, p: V3, n: V3, c: Color, gain = 1) => {
      t.p.set(p, i * 3); t.n.set(n, i * 3); t.c.set([c.r * gain, c.g * gain, c.b * gain], i * 3);
    };

    const qr = QRCode.create('BKK TENNIS - find a court, book the hour, pay by PromptPay', { errorCorrectionLevel: 'M' }).modules;
    const dark: number[] = [];
    qr.data.forEach((bit, i) => { if (bit) dark.push(i); });
    const cell = 2.9 / qr.size, QY = 2.45, BY = 2.4, BR = 1.22;
    // the seam of a tennis ball: one closed curve that swings between the two hemispheres
    const seam: V3[] = [];
    for (let i = 0; i < 160; i++) {
      const a = (i / 160) * TAU, x = Math.cos(a) * 0.74 + Math.cos(3 * a) * 0.26, y = Math.sin(a) * 0.74 - Math.sin(3 * a) * 0.26, z = Math.sin(2 * a) * 0.72;
      const l = Math.hypot(x, y, z);
      seam.push([x / l, y / l, z / l]);
    }
    const cDust = col(T.mint).multiplyScalar(0.5), cQr = col(T.surface), cBall = col(T.voltBright), cSeam = col(T.surface);
    const delay = new Float32Array(N), seed = new Float32Array(N);
    for (let i = 0; i < N; i++) {
      const mote = pr() < 0.14, rad = mote ? 1 + 4.5 * pr() : 0.15 + 4.4 * Math.sqrt(pr()), a = pr() * TAU;
      const y = mote ? 0.8 + 3.6 * pr() : (rad < 1.75 ? 0.45 : rad < 3.1 ? 0.29 : rad < 4.6 ? 0.15 : 0.02) + pr() * 0.05;
      put(A, i, [Math.cos(a) * rad, y, Math.sin(a) * rad], [0, 1, 0], cDust);

      const d = dark[Math.floor(pr() * dark.length)] ?? 0, row = Math.floor(d / qr.size), column = d % qr.size;
      put(B, i, [(column - qr.size / 2 + pr()) * cell, QY + (qr.size / 2 - row - pr()) * cell, (pr() - 0.5) * 0.07],
        [(pr() - 0.5) * 0.5, (pr() - 0.5) * 0.5, 1], cQr);

      const u = pr() * 2 - 1, th = pr() * TAU, s = Math.sqrt(1 - u * u), nx = s * Math.cos(th), ny = u, nz = s * Math.sin(th);
      const onSeam = seam.some((q) => (q[0] - nx) ** 2 + (q[1] - ny) ** 2 + (q[2] - nz) ** 2 < 0.0042);
      const fuzz = BR * (1 + (pr() - 0.5) * 0.03);
      put(C, i, [nx * fuzz, BY + ny * fuzz, nz * fuzz], [nx, ny, nz], onSeam ? cSeam : cBall);
      delay[i] = pr(); seed[i] = pr();
    }

    const geo = new BufferGeometry();
    const pos = new BufferAttribute(new Float32Array(N * 3), 3), nrm = new BufferAttribute(new Float32Array(N * 3), 3), tint = new BufferAttribute(new Float32Array(N * 3), 3);
    for (const a of [pos, nrm, tint]) a.setUsage(DynamicDrawUsage);
    geo.setAttribute('position', pos); geo.setAttribute('aNrm', nrm); geo.setAttribute('aCol', tint);
    geo.setAttribute('aSeed', new BufferAttribute(seed, 1));
    const mat = new ShaderMaterial({
      fog: false,
      uniforms: { uTime: { value: 0 }, uPx: { value: 1 } },
      vertexShader: `
        uniform float uTime, uPx; attribute vec3 aNrm, aCol; attribute float aSeed; varying vec3 vCol;
        void main(){
          vec3 p = position + vec3(sin(uTime*1.3+aSeed*40.), cos(uTime*1.1+aSeed*23.), sin(uTime*.9+aSeed*71.))*.014;
          vec4 mv = modelViewMatrix * vec4(p, 1.);
          vec3 n = normalize(mat3(modelViewMatrix) * aNrm + vec3(1e-4));
          float shade = clamp(dot(n, normalize(vec3(-.5, .7, .55)))*.62 + .46, 0., 1.);
          vCol = aCol * mix(.16, 1., shade*shade);
          gl_PointSize = max(1.5, uPx * (.017 + aSeed*.02) / -mv.z);
          gl_Position = projectionMatrix * mv;
        }`,
      fragmentShader: `
        varying vec3 vCol;
        void main(){
          vec2 c = gl_PointCoord - .5; if (dot(c, c) > .25) discard;
          gl_FragColor = vec4(vCol, 1.);
          #include <colorspace_fragment>
        }`,
    });
    const points = new Points(geo, mat);
    points.frustumCulled = false;
    g.add(points);

    let la = -1, lb = -1;
    const P = pos.array as Float32Array, Q = nrm.array as Float32Array, K = tint.array as Float32Array;
    /** out = scatter → QR (by a) → ball (by b), for one particle's three components */
    const blend = (out: Float32Array, from: Float32Array, mid: Float32Array, to: Float32Array, j: number, a: number, b: number) => {
      for (let q = j; q < j + 3; q++) { const v = lerp(from[q]!, mid[q]!, a); out[q] = v + (to[q]! - v) * b; }
    };
    const update = (ka: number, kb: number) => {
      if (ka === la && kb === lb) return;
      la = ka; lb = kb;
      for (let i = 0; i < N; i++) {
        const j = i * 3, d = delay[i]! * 0.4, a = ease(clamp((ka - d) / 0.6)), b = ease(clamp((kb - d) / 0.6)), sd = seed[i]!;
        let x = lerp(A.p[j]!, B.p[j]!, a), y = lerp(A.p[j + 1]!, B.p[j + 1]!, a), z = lerp(A.p[j + 2]!, B.p[j + 2]!, a);
        x += (C.p[j]! - x) * b; y += (C.p[j + 1]! - y) * b; z += (C.p[j + 2]! - z) * b;
        const sw = Math.sin(Math.PI * a) * (1 - b) + Math.sin(Math.PI * b);
        if (sw > 1e-3) { // swirl around the vertical axis while in transit
          const ang = sw * (1.1 + sd * 1.9), cs = Math.cos(ang), sn = Math.sin(ang), gain = 1 + sw * 0.85 * sd;
          const X = (x * cs - z * sn) * gain, Z = (x * sn + z * cs) * gain;
          x = X; z = Z; y += sw * (sd - 0.35) * 1.3;
        }
        P[j] = x; P[j + 1] = y; P[j + 2] = z;
        blend(Q, A.n, B.n, C.n, j, a, b);
        blend(K, A.c, B.c, C.c, j, a, b);
      }
      pos.needsUpdate = true; nrm.needsUpdate = true; tint.needsUpdate = true;
    };
    return { mat, points, update };
  })();

  /* ---------- drifting motes in the floodlights, shared by every stage ---------- */
  const motes = (() => {
    const n = coarse ? 700 : 1400, box = new Vector3(34, 16, 34), r = rng(3);
    const p = new Float32Array(n * 3), s = new Float32Array(n);
    for (let i = 0; i < n; i++) { p[i * 3] = (r() - 0.5) * box.x; p[i * 3 + 1] = (r() - 0.5) * box.y; p[i * 3 + 2] = (r() - 0.5) * box.z; s[i] = r(); }
    const geo = new BufferGeometry();
    geo.setAttribute('position', new BufferAttribute(p, 3));
    geo.setAttribute('aSeed', new BufferAttribute(s, 1));
    const mat = new ShaderMaterial({
      transparent: true, depthWrite: false, fog: false,
      uniforms: { uTime: { value: 0 }, uPx: { value: 1 }, uBox: { value: box }, uCenter: { value: new Vector3() }, uColor: { value: col(T.mint) } },
      vertexShader: `
        uniform float uTime, uPx; uniform vec3 uBox, uCenter; attribute float aSeed; varying float vA;
        void main(){
          vec3 p = position;
          p.y += uTime*(.05 + aSeed*.12);
          p.x += uTime*(.12 + aSeed*.2) + sin(uTime*.6 + aSeed*40.)*.35;
          p.z += sin(uTime*.4 + aSeed*17.)*.3;
          p = mod(p - uCenter + uBox*.5, uBox) - uBox*.5 + uCenter;
          vec4 mv = modelViewMatrix * vec4(p, 1.); float d = -mv.z;
          gl_PointSize = clamp(uPx*(.02 + aSeed*.035)/d, 1., 10.);
          vA = smoothstep(24., 7., d) * smoothstep(.4, 2.2, d);
          gl_Position = projectionMatrix * mv;
        }`,
      fragmentShader: `
        uniform vec3 uColor; varying float vA;
        void main(){
          vec2 c = gl_PointCoord - .5; float r = dot(c, c); if (r > .25) discard;
          gl_FragColor = vec4(uColor, vA * smoothstep(.25, .03, r) * .5);
          #include <colorspace_fragment>
        }`,
    });
    const pts = new Points(geo, mat);
    pts.frustumCulled = false;
    scene.add(pts);
    return mat;
  })();

  /* ---------- a dim night sky to reflect ---------- */
  let env: WebGLRenderTarget | null = null;
  try {
    const pmrem = new PMREMGenerator(renderer);
    const sky = new Scene(), geo = new SphereGeometry(30, 32, 16), p = geo.getAttribute('position'), c = new Float32Array(p.count * 3);
    const top: V3 = [0.07, 0.13, 0.11], hor: V3 = [0.035, 0.07, 0.055], bot: V3 = [0.004, 0.012, 0.009];
    for (let i = 0; i < p.count; i++) {
      const y = p.getY(i) / 30, to = y > 0 ? top : bot, k = Math.abs(y) ** 0.55;
      c.set([lerp(hor[0], to[0], k), lerp(hor[1], to[1], k), lerp(hor[2], to[2], k)], i * 3);
    }
    geo.setAttribute('color', new BufferAttribute(c, 3));
    const dome = new MeshBasicMaterial({ vertexColors: true, side: BackSide, fog: false });
    const panelGeo = new CircleGeometry(7, 24), panelMat = new MeshBasicMaterial({ color: new Color(1.6, 1.8, 1.6), fog: false });
    const panel = new Mesh(panelGeo, panelMat);
    panel.position.set(-14, 18, 10);
    panel.lookAt(0, 0, 0);
    sky.add(new Mesh(geo, dome), panel);
    env = pmrem.fromScene(sky, 0.03);
    scene.environment = env.texture;
    pmrem.dispose(); geo.dispose(); dome.dispose(); panelGeo.dispose(); panelMat.dispose();
  } catch { /* the lights alone still carry the scene */ }

  /* ---------- cameras ---------- */
  const CAM: readonly Cam[] = [
    { p0: [5.4, 3.1, 9.4], p1: [1.9, 1.3, 6.5], t0: [0, 0.3, 0], t1: [0, 0.45, -0.6], sx: 0.2, sy: 0.13, fog: [9, 46] },
    { p0: [0.02, 10.5, 4.6], p1: [2.8, 3.4, 7.6], t0: [0, 0, 0], t1: [0, 0, -0.4], sx: 0.17, sy: 0.1, fog: [8, 34] },
    { p0: [-3.2, 0.5, 11.5], p1: [0.5, -0.1, 9], t0: [0, 0, 0], t1: [0, 0, 0], sx: 0, sy: 0, fog: [8, 36] }, // distances come from slots.view
    { p0: [5.2, 3.4, 8.8], p1: [0.9, 2.5, 7.4], t0: [0, 2.35, 0], t1: [0, 2.45, 0], sx: 0.2, sy: 0.15, fog: [9, 44] },
    { p0: [0.9, 2.5, 7.4], p1: [-1.6, 1.8, 8.2], t0: [0, 2.45, 0], t1: [0, 2.4, 0], sx: 0.2, sy: 0.15, fog: [9, 44] },
  ];
  const zoomOut = (a: number) => (a >= 1.7 ? 1 : a >= 1 ? lerp(1.3, 1, (a - 1) / 0.7) : 1.3 + (1 - a) * 0.9);

  /* ---------- layout & scroll ---------- */
  let W = 1, H = 1, aspect = 1, tops: number[] = [], heights: number[] = [];
  const measure = () => {
    const y = window.scrollY;
    tops = o.sections.map((s) => s.getBoundingClientRect().top + y);
    heights = o.sections.map((s) => s.offsetHeight);
  };
  const resize = () => {
    const box = o.canvas.parentElement ?? o.canvas;
    W = box.clientWidth || window.innerWidth; H = box.clientHeight || window.innerHeight; aspect = W / H;
    const dpr = Math.min(window.devicePixelRatio || 1, coarse ? 1.75 : 2);
    renderer.setPixelRatio(dpr);
    renderer.setSize(W, H, false);
    camera.aspect = aspect;
    const px = (H * dpr) / (2 * Math.tan((camera.fov * Math.PI) / 360));
    motes.uniforms.uPx!.value = px; dust.mat.uniforms.uPx!.value = px;
    slots.layout(aspect);
    measure();
  };
  const rangeOf = (k: number) => (k === LAST ? Math.max(1, (heights[k] ?? 1) - H) : heights[k] ?? 1);
  const locate = (y: number) => {
    let i = LAST;
    for (let k = 0; k <= LAST; k++) if (y < (tops[k] ?? 0) + (heights[k] ?? 0)) { i = k; break; }
    return { i, t: clamp((y - (tops[i] ?? 0)) / rangeOf(i)) };
  };
  const goTo = (k: number) => {
    if (k < 0 || k > LAST) return;
    window.scrollTo({ top: (tops[k] ?? 0) + (k ? rangeOf(k) * 0.6 : 0), behavior: reduced ? 'auto' : 'smooth' });
  };

  let mx = 0, my = 0;
  const onPointer = (e: PointerEvent) => { if (e.pointerType === 'mouse') { mx = (e.clientX / W) * 2 - 1; my = (e.clientY / H) * 2 - 1; } };

  /* ---------- per frame ---------- */
  const pos = new Vector3(), tgt = new Vector3(), right = new Vector3(), up = new Vector3(0, 1, 0), v3 = new Vector3(), ballEnd = new Vector3();
  const BALL_A: V3 = [-0.9, 1.05, -4.5], BALL_B: V3 = [0.7, 0.085, 2.2];
  let last = performance.now(), time = 0, cur = window.scrollY, pmx = 0, pmy = 0, raf = 0, alive = true, activeStage = -1;
  const t0 = last;
  let sent: IntroTick | null = null;

  const frame = (now: number) => {
    if (!alive) return;
    raf = requestAnimationFrame(frame);
    const dt = Math.min(0.05, (now - last) / 1000);
    last = now;
    if (!reduced) time += dt;
    const y = window.scrollY;
    cur = reduced ? y : cur + (y - cur) * (1 - Math.exp(-dt * 7));
    if (Math.abs(y - cur) < 0.1) cur = y;
    const { i, t } = locate(cur), st = STAGE[i]!, c = CAM[i]!, e = ease(t), follow = reduced ? 1 : 1 - Math.exp(-dt * 8);

    if (st !== activeStage) { stages.forEach((g, k) => { g.visible = k === st; }); activeStage = st; }
    const show = SHOW[i]!, shown = t >= show[0] && t <= show[1];

    let veil = reduced ? 0 : 1 - sstep(0.15, 1.5, (now - t0) / 1000);
    if (i < LAST && STAGE[i + 1] !== st) veil = Math.max(veil, sstep(0.84, 1, t));
    if (i > 0 && STAGE[i - 1] !== st) veil = Math.max(veil, 1 - sstep(0, 0.16, t));
    o.veil.style.opacity = veil.toFixed(3);

    // camera: a dolly per chapter, pulled back on narrow screens, with the subject pushed clear of the text
    const pz = zoomOut(aspect);
    pos.set(lerp(c.p0[0], c.p1[0], e), lerp(c.p0[1], c.p1[1], e), lerp(c.p0[2], c.p1[2], e));
    tgt.set(lerp(c.t0[0], c.t1[0], e), lerp(c.t0[1], c.t1[1], e), lerp(c.t0[2], c.t1[2], e));
    if (st === 2) { // the matrix is framed by its own width, and sits above the text
      const d = slots.view.dist;
      tgt.set(0, -slots.view.drop, 0);
      pos.set(pos.x, pos.y - slots.view.drop, d * lerp(1.3, 1, e));
    } else {
      pos.sub(tgt).multiplyScalar(pz).add(tgt);
    }
    pmx += (mx - pmx) * (1 - Math.exp(-dt * 3)); pmy += (my - pmy) * (1 - Math.exp(-dt * 3));
    right.subVectors(tgt, pos).cross(up).normalize();
    pos.addScaledVector(right, pmx * 0.45);
    pos.y -= pmy * 0.25;
    if (st !== 2) pos.y = Math.max(0.35, pos.y); // every stage but the floating slot matrix has a floor
    camera.position.copy(pos);
    camera.lookAt(tgt);
    if (aspect >= 1) camera.setViewOffset(W, H, -c.sx * W, c.sx ? 0.05 * H : 0, W, H); else camera.setViewOffset(W, H, 0, c.sy * H, W, H);
    camera.updateMatrixWorld();
    if (st === 2) { fog.near = slots.view.dist * 0.75; fog.far = slots.view.dist * 3.2; } else { fog.near = c.fog[0] * pz; fog.far = c.fog[1] * pz; }
    motes.uniforms.uTime!.value = time;
    (motes.uniforms.uCenter!.value as Vector3).copy(tgt);

    let radiusKm = 0, picked = 0, holdSec = 600, qr = i === 4 ? 100 : 0;
    if (st === 0) {
      // the serve: far baseline → over the net → one bounce in the service box → past the lens
      const u = sstep(0.06, 0.94, t);
      ballEnd.copy(pos).addScaledVector(right, -0.55).add(v3.subVectors(tgt, pos).normalize().multiplyScalar(0.9));
      ballEnd.y -= 0.25;
      const b = court.ball.position;
      if (u < 0.62) {
        const k = u / 0.62;
        b.set(lerp(BALL_A[0], BALL_B[0], k), lerp(BALL_A[1], BALL_B[1], k) + 4.6 * k * (1 - k), lerp(BALL_A[2], BALL_B[2], k));
      } else {
        const k = (u - 0.62) / 0.38;
        b.set(lerp(BALL_B[0], ballEnd.x, k), lerp(BALL_B[1], ballEnd.y, k) + 2 * k * (1 - k), lerp(BALL_B[2], ballEnd.z, k));
      }
      court.ball.rotation.x = -u * 22;
      court.glow.position.copy(b);
      court.shadow.position.set(b.x, 0.018, b.z);
      const sh = clamp(1 - b.y / 2.2);
      court.shadow.scale.setScalar(0.5 + (1 - sh) * 1.6);
      (court.shadow.material as MeshBasicMaterial).opacity = 0.55 * sh;
    } else if (st === 1) {
      // the radius grows 3 → 5 → 10 km, resting on each step
      const R = t < 0.34 ? lerp(0, 3, sstep(0.1, 0.28, t)) : t < 0.58 ? lerp(3, 5, sstep(0.38, 0.5, t)) : lerp(5, 10, sstep(0.62, 0.78, t));
      radiusKm = R >= 9.99 ? 10 : R >= 4.99 ? 5 : R >= 2.99 ? 3 : 0;
      map.sweep.scale.setScalar(Math.max(0.001, R * KM));
      (map.sweep.material as MeshBasicMaterial).opacity = R > 0.02 ? 0.95 : 0;
      map.area.scale.setScalar(Math.max(0.001, R * KM));
      (map.area.material as MeshBasicMaterial).opacity = R > 0.02 ? 0.07 : 0;
      map.rings.forEach((ring, k) => { (ring.material as MeshBasicMaterial).opacity = R >= RADII_KM[k]! - 0.01 ? 0.4 : 0; });
      map.youGlow.scale.setScalar(1.1 + Math.sin(time * 2.4) * 0.18);
      map.pins.forEach((p) => {
        const lit = p.km <= R;
        p.k += ((lit ? 1 : 0) - p.k) * follow;
        const mat = lit ? M.pinLit : M.pinDim;
        if (p.head.material !== mat) { p.head.material = mat; p.stem.material = mat; }
        p.head.scale.setScalar(1 + 0.4 * p.k);
        (p.glow.material as SpriteMaterial).opacity = 0.85 * p.k;
      });
      RADII_KM.forEach((km, k) => {
        const tag = o.tags[k];
        if (!tag) return;
        v3.set(Math.cos(-0.5) * km * KM, 0.42, Math.sin(-0.5) * km * KM).project(camera);
        tag.style.transform = `translate3d(${((v3.x * 0.5 + 0.5) * W + 8).toFixed(1)}px,${((-v3.y * 0.5 + 0.5) * H - 10).toFixed(1)}px,0)`;
        tag.style.opacity = shown && R >= km - 0.01 ? '1' : '0';
      });
    } else if (st === 2) {
      const on = [t >= 0.26, t >= 0.44];
      slots.picks.forEach((p, k) => {
        const want = on[k] === true;
        if (want !== p.on) { p.on = want; p.tile.mesh.material = want ? p.picked : p.free; p.rim.visible = want; }
        p.k += ((want ? 1 : 0) - p.k) * follow;
        const z = p.k * 0.42;
        p.tile.mesh.position.z = z; p.rim.position.z = z - 0.07; p.glow.position.z = z - 0.2;
        (p.glow.material as SpriteMaterial).opacity = 0.5 * p.k;
        if (want) picked++;
      });
      if (on[0]) holdSec = Math.round(600 - sstep(0.26, 1, t) * 97);
    } else {
      const ka = i === 3 ? sstep(0.1, 0.62, t) : 1, kb = i === 4 ? sstep(0.04, 0.5, t) : 0;
      dust.update(ka, kb);
      dust.mat.uniforms.uTime!.value = time;
      dust.points.rotation.y = Math.sin(time * 0.4) * lerp(0.28, 0.9, kb) + kb * 1.2;
      if (i === 3) qr = Math.round(ka * 100);
    }
    if (st !== 1) o.tags.forEach((tag) => { tag.style.opacity = '0'; });

    const progress = Math.round(((i + t) / STAGE.length) * 1000) / 1000;
    if (!sent || sent.chapter !== i || sent.shown !== shown || sent.progress !== progress || sent.radiusKm !== radiusKm
      || sent.picked !== picked || sent.holdSec !== holdSec || sent.qr !== qr) {
      sent = { chapter: i, shown, progress, radiusKm, picked, holdSec, qr };
      o.onTick(sent);
    }
    renderer.render(scene, camera);
  };

  const onVisibility = () => {
    cancelAnimationFrame(raf);
    if (alive && !document.hidden) { last = performance.now(); raf = requestAnimationFrame(frame); }
  };
  window.addEventListener('resize', resize);
  window.addEventListener('pointermove', onPointer, { passive: true });
  document.addEventListener('visibilitychange', onVisibility);
  // tile faces use the display face; it may still be loading on a first visit
  void document.fonts?.load('700 92px "Space Grotesk"').then(() => { if (alive) { slots.repaint(); measure(); } });

  resize();
  frame(performance.now()); // first frame now, so the hero is drawn before the browser's first paint

  return {
    goTo,
    destroy() {
      alive = false;
      cancelAnimationFrame(raf);
      window.removeEventListener('resize', resize);
      window.removeEventListener('pointermove', onPointer);
      document.removeEventListener('visibilitychange', onVisibility);
      const seen = new Set<Material>();
      scene.traverse((obj) => {
        const m = obj as Partial<Mesh>;
        m.geometry?.dispose();
        for (const mat of Array.isArray(m.material) ? m.material : m.material ? [m.material] : []) seen.add(mat);
      });
      seen.forEach((m) => m.dispose());
      textures.forEach((t) => t.dispose());
      env?.dispose();
      renderer.dispose();
      renderer.forceContextLoss();
      o.root.style.removeProperty('--intro-night');
    },
  };
}
