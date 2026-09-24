// Minimal static server with history-mode fallback, for testing a built SPA/PWA (`quasar serve` needs the global CLI).
//   node scripts/serve-spa.mjs <dir> <port>
import { createServer } from 'node:http';
import { existsSync, readFileSync, statSync } from 'node:fs';
import { extname, join, normalize, resolve } from 'node:path';

const [dir = '.', port = '9103'] = process.argv.slice(2);
const root = resolve(dir);
const TYPES = {
  '.html': 'text/html; charset=utf-8', '.js': 'text/javascript; charset=utf-8', '.mjs': 'text/javascript; charset=utf-8',
  '.css': 'text/css; charset=utf-8', '.json': 'application/json; charset=utf-8', '.webmanifest': 'application/manifest+json',
  '.png': 'image/png', '.svg': 'image/svg+xml', '.ico': 'image/x-icon', '.woff2': 'font/woff2', '.woff': 'font/woff', '.map': 'application/json',
};

createServer((req, res) => {
  const path = normalize(decodeURIComponent(new URL(req.url ?? '/', 'http://x').pathname)).replace(/^([/\\])+/, '');
  let file = join(root, path);
  if (!file.startsWith(root)) { res.writeHead(403).end(); return; }
  if (!existsSync(file) || statSync(file).isDirectory()) {
    // real files 404, but any extension-less path is a client-side route
    if (extname(path)) { res.writeHead(404).end('not found'); return; }
    file = join(root, 'index.html');
  }
  res.writeHead(200, {
    'Content-Type': TYPES[extname(file)] ?? 'application/octet-stream',
    // the service worker must always be revalidated so updates are picked up
    'Cache-Control': file.endsWith('sw.js') || file.endsWith('index.html') ? 'no-cache' : 'public, max-age=3600',
  });
  res.end(readFileSync(file));
}).listen(Number(port), 'localhost', () => console.log(`serving ${root} on http://localhost:${port}`));
