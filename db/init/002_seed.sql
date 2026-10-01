-- Sample data for local development only (venue names/coordinates are illustrative).

INSERT INTO users (id, phone, display_name, role) VALUES
  ('00000000-0000-0000-0000-000000000001', '+66800000001', 'Platform Admin', 'platform_admin'),
  ('00000000-0000-0000-0000-000000000002', '+66800000002', 'Racquet Club Owner', 'partner_owner'),
  ('00000000-0000-0000-0000-000000000003', '+66800000003', 'สมชาย ก.', 'player');

INSERT INTO partners (id, legal_name, status) VALUES
  ('10000000-0000-0000-0000-000000000001', 'Racquet Club Sukhumvit Co., Ltd.', 'active'),
  ('10000000-0000-0000-0000-000000000002', 'Benchasiri Sports Community', 'active'),
  ('10000000-0000-0000-0000-000000000003', 'Asoke Sports Club Co., Ltd.', 'active'),
  ('10000000-0000-0000-0000-000000000004', 'Ari Tennis Hub', 'active'),
  ('10000000-0000-0000-0000-000000000005', 'Bang Na Tennis Center', 'pending');

INSERT INTO venues (id, partner_id, slug, name_th, name_en, district, address, lat, lng, amenities, transit_note, rating_avg, rating_count) VALUES
  ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', 'racquet-club-sukhumvit-49',
   'เดอะ แร็กเก็ต คลับ สุขุมวิท 49', 'The Racquet Club Sukhumvit 49', 'วัฒนา / Watthana',
   'สุขุมวิท 49, คลองตันเหนือ, วัฒนา', 13.7358, 100.5783,
   '["floodlit","parking","shower","pro_shop","air_con"]', 'ขับรถจาก BTS พร้อมพงษ์ 5 นาที', 4.9, 320),
  ('20000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002', 'benchasiri-park',
   'คอร์ทเทนนิส สวนเบญจสิริ', 'Benchasiri Park Tennis Court', 'คลองเตย / Khlong Toei',
   'สุขุมวิท 22-24, คลองตัน, คลองเตย', 13.7303, 100.5683,
   '["floodlit"]', 'ติด BTS พร้อมพงษ์ (เดิน 2 นาที)', 4.6, 188),
  ('20000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', 'asoke-sports-club',
   'อโศก สปอร์ต คลับ', 'Asoke Sports Club & Tennis', 'วัฒนา / Watthana',
   'อโศกมนตรี ซอย 1, คลองเตยเหนือ, วัฒนา', 13.7419, 100.5617,
   '["floodlit","parking","shower"]', 'MRT สุขุมวิท / BTS อโศก', 4.8, 142),
  ('20000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000004', 'ari-tennis-hub',
   'อารีย์ เทนนิส ฮับ', 'Ari Tennis Hub', 'พญาไท / Phaya Thai',
   'พหลโยธิน 7, สามเสนใน, พญาไท', 13.7797, 100.5446,
   '["floodlit","shower"]', 'BTS อารีย์ (เดิน 6 นาที)', 4.5, 76);

INSERT INTO courts (id, venue_id, name, surface, indoor, air_con, floodlit, sort_order) VALUES
  ('30000000-0000-0000-0000-000000000011', '20000000-0000-0000-0000-000000000001', 'คอร์ท 1', 'hard', true,  true,  true, 1),
  ('30000000-0000-0000-0000-000000000012', '20000000-0000-0000-0000-000000000001', 'คอร์ท 2', 'hard', true,  true,  true, 2),
  ('30000000-0000-0000-0000-000000000013', '20000000-0000-0000-0000-000000000001', 'คอร์ท 3', 'clay', true,  true,  true, 3),
  ('30000000-0000-0000-0000-000000000014', '20000000-0000-0000-0000-000000000001', 'คอร์ท 4', 'hard', false, false, true, 4),
  ('30000000-0000-0000-0000-000000000015', '20000000-0000-0000-0000-000000000001', 'คอร์ท 5', 'hard', false, false, true, 5),
  ('30000000-0000-0000-0000-000000000021', '20000000-0000-0000-0000-000000000002', 'คอร์ท A', 'hard', false, false, true, 1),
  ('30000000-0000-0000-0000-000000000022', '20000000-0000-0000-0000-000000000002', 'คอร์ท B', 'hard', false, false, true, 2),
  ('30000000-0000-0000-0000-000000000031', '20000000-0000-0000-0000-000000000003', 'Clay 1',  'clay', false, false, true, 1),
  ('30000000-0000-0000-0000-000000000032', '20000000-0000-0000-0000-000000000003', 'Clay 2',  'clay', false, false, true, 2),
  ('30000000-0000-0000-0000-000000000041', '20000000-0000-0000-0000-000000000004', 'คอร์ท 1', 'hard', false, false, true, 1);

-- Every venue open 06:00–22:00 every day, 60-minute slots.
INSERT INTO operating_hours (venue_id, day_of_week, open_time, close_time, slot_minutes)
SELECT v.id, d, '06:00', '22:00', 60
FROM venues v CROSS JOIN generate_series(0, 6) AS d;

INSERT INTO price_rules (venue_id, day_type, start_time, end_time, price_per_hour, label, priority) VALUES
  ('20000000-0000-0000-0000-000000000001', 'any', '06:00', '17:00', 450, 'Standard', 0),
  ('20000000-0000-0000-0000-000000000001', 'any', '17:00', '18:00', 600, 'Prime Twilight Slot', 10),
  ('20000000-0000-0000-0000-000000000001', 'any', '18:00', '20:00', 650, 'Floodlit Peak Hour', 10),
  ('20000000-0000-0000-0000-000000000001', 'any', '20:00', '22:00', 550, 'Night Match', 10),
  ('20000000-0000-0000-0000-000000000002', 'any', '06:00', '22:00', 150, 'ราคาประหยัด', 0),
  ('20000000-0000-0000-0000-000000000003', 'any', '06:00', '22:00', 500, 'Red Clay', 0),
  ('20000000-0000-0000-0000-000000000004', 'any', '06:00', '17:00', 300, 'Day', 0),
  ('20000000-0000-0000-0000-000000000004', 'any', '17:00', '22:00', 400, 'Evening', 10);

INSERT INTO addons (venue_id, name, description, icon, price, unit) VALUES
  ('20000000-0000-0000-0000-000000000001', 'เช่าไม้เทนนิส Wilson / Babolat', 'เลือกกริปและน้ำหนักได้ที่หน้าคอร์ท', 'sports_tennis', 100, 'per_booking'),
  ('20000000-0000-0000-0000-000000000001', 'ซื้อลูกเทนนิส 1 กระป๋อง', 'Dunlop Fort All Court (3 ลูก/กระป๋องใหม่)', 'sports_baseball', 220, 'per_booking'),
  ('20000000-0000-0000-0000-000000000001', 'บริการ Knocker ซ้อมตี', 'คู่ซ้อมระดับนักกีฬาคอยป้อนลูกมาตรฐาน', 'sports', 350, 'per_hour'),
  ('20000000-0000-0000-0000-000000000003', 'เช่าไม้เทนนิส', NULL, 'sports_tennis', 80, 'per_booking');

INSERT INTO venue_members (venue_id, user_id, role) VALUES
  ('20000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000002', 'owner');
