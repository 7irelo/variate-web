-- Loads the data.sql catalogue into the storefront schema (dbo.products).
-- Generated from data.sql; only products with an image in wwwroot/img/products.
-- Idempotent: products already present (by name) are skipped.
-- Sale prices are 15% off, rounded down to end in 99.
SET NOCOUNT ON;
INSERT INTO dbo.products (CategoryId, Brand, Name, Description, Price, DiscountedPrice, ReleaseDate, ImageUrl, Stock, IsFeatured, OnSale, CreatedAt, UpdatedAt)
SELECT v.CategoryId, N'', v.Name, v.Description, v.Price, v.DiscountedPrice, v.ReleaseDate, v.ImageUrl, 100, 0, v.OnSale, SYSUTCDATETIME(), SYSUTCDATETIME()
FROM (VALUES
    (1, N'HP Envy 13', N'A slim and powerful laptop with a vibrant display.', 15999, 13499, '2024-06-01', N'hp_envy_13.jpg', 1),
    (1, N'Bose SoundLink Revolve', N'Portable Bluetooth speaker with 360-degree sound.', 2499, 2499, '2024-07-01', N'bose_soundlink_revolve.jpg', 0),
    (1, N'Amazon Echo Show 10', N'Smart display with Alexa and a rotating screen.', 8999, 8999, '2024-08-01', N'amazon_echo_show_10.jpg', 0),
    (1, N'GoPro Hero 10', N'Action camera with 5K video and HyperSmooth stabilization.', 15999, 15999, '2024-09-01', N'gopro_hero_10.jpg', 0),
    (1, N'Garmin Fenix 7', N'Multisport GPS watch with advanced fitness tracking.', 25999, 21999, '2024-10-01', N'garmin_fenix_7.jpg', 1),
    (2, N'Breville Smart Oven', N'Countertop oven with Element IQ technology.', 5999, 5999, '2024-06-01', N'breville_smart_oven.jpg', 0),
    (2, N'Vitamix E310 Blender', N'High-performance blender for smoothies and more.', 4799, 3999, '2024-07-01', N'vitamix_e310_blender.jpg', 1),
    (2, N'Anova Sous Vide Precision Cooker', N'Precision cooker for restaurant-quality meals at home.', 2999, 2999, '2024-08-01', N'anova_sous_vide.jpg', 0),
    (2, N'Le Creuset Dutch Oven', N'Iconic enameled cast iron for all your cooking needs.', 3299, 2799, '2024-09-01', N'le_creuset_dutch_oven.jpg', 1),
    (2, N'Cuisinart Ice Cream Maker', N'Easy-to-use ice cream maker for homemade treats.', 999, 999, '2024-10-01', N'cuisinart_ice_cream_maker.jpg', 0),
    (3, N'Hermès Birkin Bag', N'Luxury leather handbag for a statement of elegance.', 129999, 110399, '2024-06-01', N'hermes_birkin.jpg', 1),
    (3, N'Tom Ford Sunglasses', N'Stylish sunglasses with UV protection.', 4999, 4999, '2024-07-01', N'tom_ford_sunglasses.jpg', 0),
    (3, N'Omega Seamaster Watch', N'Classic luxury watch for any occasion.', 35999, 30499, '2024-08-01', N'omega_seamaster.jpg', 1),
    (3, N'Lululemon Align Leggings', N'Comfortable and versatile leggings for everyday wear.', 999, 999, '2024-09-01', N'lululemon_align_leggings.jpg', 0),
    (3, N'Dyson Airwrap', N'Versatile hair styling tool with multiple attachments.', 5999, 4999, '2024-10-01', N'dyson_airwrap.jpg', 1),
    (4, N'Fender American Professional II Stratocaster', N'Iconic electric guitar with a modern twist.', 15999, 15999, '2024-06-01', N'fender_stratocaster.jpg', 0),
    (4, N'Yamaha YDP-144 Digital Piano', N'Realistic piano experience with GHS action.', 49999, 42399, '2024-07-01', N'yamaha_ydp144.jpg', 1),
    (4, N'Pearl Export EXX Drum Set', N'Complete drum set for beginners and intermediate players.', 29999, 29999, '2024-08-01', N'pearl_export_exx.jpg', 0),
    (4, N'DW Collector''s Series Drum Kit', N'High-end drum kit with a custom finish.', 79999, 67899, '2024-09-01', N'dw_collectors_drum_kit.jpg', 1),
    (4, N'Gibson Les Paul Standard', N'Classic electric guitar with a solid body.', 24999, 24999, '2024-10-01', N'gibson_les_paul.jpg', 0),
    (5, N'Prismacolor Premier Colored Pencils', N'Soft core colored pencils for smooth, rich color.', 1999, 1599, '2024-06-01', N'prismacolor_premier.jpg', 1),
    (5, N'Liquitex Professional Acrylic Paints', N'High-quality acrylic paints for artists.', 2499, 2499, '2024-07-01', N'liquitex_acrylic_paints.jpg', 0),
    (5, N'Wacom Intuos Pro Tablet', N'Pen tablet for digital art and design.', 12999, 10999, '2024-08-01', N'wacom_intuos_pro.jpg', 1),
    (5, N'Golden Heavy Body Acrylics', N'Thick, richly pigmented acrylic paint for professional artists.', 3999, 3999, '2024-09-01', N'golden_acrylics.jpg', 0),
    (5, N'Albrecht Dürer Watercolor Pencils', N'High-quality watercolor pencils for detailed work.', 2999, 2499, '2024-10-01', N'albrecht_durer_watercolor_pencils.jpg', 1),
    (6, N'Graco 4Ever Car Seat', N'Convertible car seat that grows with your child.', 4999, 4199, '2024-06-01', N'graco_4ever_car_seat.jpg', 1),
    (6, N'Fisher-Price Jumperoo', N'Interactive jumper for active playtime.', 2999, 2999, '2024-07-01', N'fisher_price_jumperoo.jpg', 0),
    (6, N'Nuna Mixx Stroller', N'Luxury stroller with all-terrain wheels and sleek design.', 8999, 7599, '2024-08-01', N'nuna_mixx_stroller.jpg', 1),
    (6, N'Owlet Smart Sock', N'Monitor your baby''s heart rate and oxygen levels.', 3999, 3999, '2024-09-01', N'owlet_smart_sock.jpg', 0),
    (6, N'Ergobaby Omni 360 Carrier', N'Comfortable and versatile baby carrier for all positions.', 2999, 2499, '2024-10-01', N'ergobaby_omni_360.jpg', 1),
    (7, N'Tempur-Pedic Memory Foam Pillow', N'Contour pillow for restful sleep.', 2999, 2499, '2024-06-01', N'tempur_pedic_pillow.jpg', 1),
    (7, N'Brooklinen Luxe Core Sheet Set', N'Soft and durable cotton sheets for a luxurious sleep experience.', 4999, 4999, '2024-07-01', N'brooklinen_luxe_core.jpg', 0),
    (7, N'Parachute Home Classic Towels', N'Absorbent and plush towels for everyday use.', 1999, 1599, '2024-08-01', N'parachute_classic_towels.jpg', 1),
    (7, N'Boll & Branch Down Comforter', N'Fluffy and warm down comforter for chilly nights.', 8999, 8999, '2024-09-01', N'boll_branch_down_comforter.jpg', 0),
    (7, N'Ugg Blissful Throw Blanket', N'Cozy and soft throw blanket for lounging.', 1999, 1599, '2024-10-01', N'ugg_blissful_throw.jpg', 1),
    (8, N'West Elm Andes Sofa', N'Modern and elegant sofa with customizable fabrics.', 32999, 32999, '2024-06-01', N'west_elm_andes_sofa.jpg', 0),
    (8, N'IKEA Billy Bookcase', N'Affordable and versatile bookcase for any room.', 1999, 1599, '2024-07-01', N'ikea_billy_bookcase.jpg', 1),
    (8, N'Herman Miller Aeron Chair', N'Ergonomic office chair with adjustable features.', 12999, 12999, '2024-08-01', N'herman_miller_aeron.jpg', 0),
    (8, N'CB2 Peekaboo Acrylic Coffee Table', N'Stylish and minimalist coffee table.', 2999, 2499, '2024-09-01', N'cb2_peekaboo_coffee_table.jpg', 1),
    (8, N'Article Seno Dining Table', N'Mid-century modern dining table with walnut finish.', 4999, 4999, '2024-10-01', N'article_seno_dining_table.jpg', 0)
) AS v (CategoryId, Name, Description, Price, DiscountedPrice, ReleaseDate, ImageUrl, OnSale)
WHERE NOT EXISTS (SELECT 1 FROM dbo.products p WHERE p.Name = v.Name);
