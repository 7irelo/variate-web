INSERT INTO dbo.products (CategoryId, Name, Description, Price, Release, ImageUrl, OnSale) VALUES
-- Electronics
(1, 'HP Envy 13', 'A slim and powerful laptop with a vibrant display.', 15999, '2024-06-01', 'hp_envy_13.jpg', 1),
(1, 'Bose SoundLink Revolve', 'Portable Bluetooth speaker with 360-degree sound.', 2499, '2024-07-01', 'bose_soundlink_revolve.jpg', 0),
(1, 'Amazon Echo Show 10', 'Smart display with Alexa and a rotating screen.', 8999, '2024-08-01', 'amazon_echo_show_10.jpg', 0),
(1, 'GoPro Hero 10', 'Action camera with 5K video and HyperSmooth stabilization.', 15999, '2024-09-01', 'gopro_hero_10.jpg', 0),
(1, 'Garmin Fenix 7', 'Multisport GPS watch with advanced fitness tracking.', 25999, '2024-10-01', 'garmin_fenix_7.jpg', 1),

-- Home and Kitchen
(2, 'Breville Smart Oven', 'Countertop oven with Element IQ technology.', 5999, '2024-06-01', 'breville_smart_oven.jpg', 0),
(2, 'Vitamix E310 Blender', 'High-performance blender for smoothies and more.', 4799, '2024-07-01', 'vitamix_e310_blender.jpg', 1),
(2, 'Anova Sous Vide Precision Cooker', 'Precision cooker for restaurant-quality meals at home.', 2999, '2024-08-01', 'anova_sous_vide.jpg', 0),
(2, 'Le Creuset Dutch Oven', 'Iconic enameled cast iron for all your cooking needs.', 3299, '2024-09-01', 'le_creuset_dutch_oven.jpg', 1),
(2, 'Cuisinart Ice Cream Maker', 'Easy-to-use ice cream maker for homemade treats.', 999, '2024-10-01', 'cuisinart_ice_cream_maker.jpg', 0),

-- Fashion and Beauty
(3, 'Hermès Birkin Bag', 'Luxury leather handbag for a statement of elegance.', 129999, '2024-06-01', 'hermes_birkin.jpg', 1),
(3, 'Tom Ford Sunglasses', 'Stylish sunglasses with UV protection.', 4999, '2024-07-01', 'tom_ford_sunglasses.jpg', 0),
(3, 'Omega Seamaster Watch', 'Classic luxury watch for any occasion.', 35999, '2024-08-01', 'omega_seamaster.jpg', 1),
(3, 'Lululemon Align Leggings', 'Comfortable and versatile leggings for everyday wear.', 999, '2024-09-01', 'lululemon_align_leggings.jpg', 0),
(3, 'Dyson Airwrap', 'Versatile hair styling tool with multiple attachments.', 5999, '2024-10-01', 'dyson_airwrap.jpg', 1),

-- Musical Instruments
(4, 'Fender American Professional II Stratocaster', 'Iconic electric guitar with a modern twist.', 15999, '2024-06-01', 'fender_stratocaster.jpg', 0),
(4, 'Yamaha YDP-144 Digital Piano', 'Realistic piano experience with GHS action.', 49999, '2024-07-01', 'yamaha_ydp144.jpg', 1),
(4, 'Pearl Export EXX Drum Set', 'Complete drum set for beginners and intermediate players.', 29999, '2024-08-01', 'pearl_export_exx.jpg', 0),
(4, 'DW Collector''s Series Drum Kit', 'High-end drum kit with a custom finish.', 79999, '2024-09-01', 'dw_collectors_drum_kit.jpg', 1),
(4, 'Gibson Les Paul Standard', 'Classic electric guitar with a solid body.', 24999, '2024-10-01', 'gibson_les_paul.jpg', 0),

-- Art and Crafts
(5, 'Prismacolor Premier Colored Pencils', 'Soft core colored pencils for smooth, rich color.', 1999, '2024-06-01', 'prismacolor_premier.jpg', 1),
(5, 'Liquitex Professional Acrylic Paints', 'High-quality acrylic paints for artists.', 2499, '2024-07-01', 'liquitex_acrylic_paints.jpg', 0),
(5, 'Wacom Intuos Pro Tablet', 'Pen tablet for digital art and design.', 12999, '2024-08-01', 'wacom_intuos_pro.jpg', 1),
(5, 'Golden Heavy Body Acrylics', 'Thick, richly pigmented acrylic paint for professional artists.', 3999, '2024-09-01', 'golden_acrylics.jpg', 0),
(5, 'Albrecht Dürer Watercolor Pencils', 'High-quality watercolor pencils for detailed work.', 2999, '2024-10-01', 'albrecht_durer_watercolor_pencils.jpg', 1),

-- Baby and Toddler
(6, 'Graco 4Ever Car Seat', 'Convertible car seat that grows with your child.', 4999, '2024-06-01', 'graco_4ever_car_seat.jpg', 1),
(6, 'Fisher-Price Jumperoo', 'Interactive jumper for active playtime.', 2999, '2024-07-01', 'fisher_price_jumperoo.jpg', 0),
(6, 'Nuna Mixx Stroller', 'Luxury stroller with all-terrain wheels and sleek design.', 8999, '2024-08-01', 'nuna_mixx_stroller.jpg', 1),
(6, 'Owlet Smart Sock', 'Monitor your baby''s heart rate and oxygen levels.', 3999, '2024-09-01', 'owlet_smart_sock.jpg', 0),
(6, 'Ergobaby Omni 360 Carrier', 'Comfortable and versatile baby carrier for all positions.', 2999, '2024-10-01', 'ergobaby_omni_360.jpg', 1),

-- Bed and Bath
(7, 'Tempur-Pedic Memory Foam Pillow', 'Contour pillow for restful sleep.', 2999, '2024-06-01', 'tempur_pedic_pillow.jpg', 1),
(7, 'Brooklinen Luxe Core Sheet Set', 'Soft and durable cotton sheets for a luxurious sleep experience.', 4999, '2024-07-01', 'brooklinen_luxe_core.jpg', 0),
(7, 'Parachute Home Classic Towels', 'Absorbent and plush towels for everyday use.', 1999, '2024-08-01', 'parachute_classic_towels.jpg', 1),
(7, 'Boll & Branch Down Comforter', 'Fluffy and warm down comforter for chilly nights.', 8999, '2024-09-01', 'boll_branch_down_comforter.jpg', 0),
(7, 'Ugg Blissful Throw Blanket', 'Cozy and soft throw blanket for lounging.', 1999, '2024-10-01', 'ugg_blissful_throw.jpg', 1),

-- Decor and Furniture
(8, 'West Elm Andes Sofa', 'Modern and elegant sofa with customizable fabrics.', 32999, '2024-06-01', 'west_elm_andes_sofa.jpg', 0),
(8, 'IKEA Billy Bookcase', 'Affordable and versatile bookcase for any room.', 1999, '2024-07-01', 'ikea_billy_bookcase.jpg', 1),
(8, 'Herman Miller Aeron Chair', 'Ergonomic office chair with adjustable features.', 12999, '2024-08-01', 'herman_miller_aeron.jpg', 0),
(8, 'CB2 Peekaboo Acrylic Coffee Table', 'Stylish and minimalist coffee table.', 2999, '2024-09-01', 'cb2_peekaboo_coffee_table.jpg', 1),
(8, 'Article Seno Dining Table', 'Mid-century modern dining table with walnut finish.', 4999, '2024-10-01', 'article_seno_dining_table.jpg', 0),

-- Health and Beauty
(9, 'Fitbit Charge 5', 'Advanced fitness and health tracker with built-in GPS.', 2999, '2024-06-01', 'fitbit_charge_5.jpg', 1),
(9, 'Philips Sonicare DiamondClean', 'Electric toothbrush with advanced cleaning technology.', 2499, '2024-07-01', 'philips_sonicare_diamondclean.jpg', 0),
(9, 'Clarisonic Mia Smart', 'Facial cleansing brush with multiple attachments.', 1999, '2024-08-01', 'clarisonic_mia_smart.jpg', 1),
(9, 'Revlon One-Step Hair Dryer', 'Hot air brush for styling and drying hair in one step.', 999, '2024-09-01', 'revlon_one_step.jpg', 0),
(9, 'Foreo Luna 3', 'Smart facial cleansing device with T-Sonic pulsations.', 3999, '2024-10-01', 'foreo_luna_3.jpg', 1),

-- Home and Garden
(10, 'Weber Genesis II Grill', 'High-performance gas grill for outdoor cooking.', 8999, '2024-06-01', 'weber_genesis_ii_grill.jpg', 1),
(10, 'Husqvarna Automower', 'Robotic lawn mower for a perfectly manicured lawn.', 15999, '2024-07-01', 'husqvarna_automower.jpg', 0),
(10, 'Sun Joe Electric Pressure Washer', 'Powerful pressure washer for cleaning outdoors.', 2999, '2024-08-01', 'sun_joe_pressure_washer.jpg', 1),
(10, 'Nest Learning Thermostat', 'Smart thermostat that learns your schedule and saves energy.', 2499, '2024-09-01', 'nest_learning_thermostat.jpg', 0),
(10, 'Philips Hue Outdoor Lightstrip', 'Weatherproof LED light strip for outdoor ambiance.', 1999, '2024-10-01', 'philips_hue_outdoor_lightstrip.jpg', 1),

-- Jewellery and Watches
(11, 'Rolex Submariner', 'Luxury dive watch with impeccable craftsmanship.', 89999, '2024-06-01', 'rolex_submariner.jpg', 0),
(11, 'Cartier Love Bracelet', 'Iconic bracelet symbolizing eternal love.', 59999, '2024-07-01', 'cartier_love_bracelet.jpg', 1),
(11, 'Pandora Charm Bracelet', 'Customizable charm bracelet with endless possibilities.', 3999, '2024-08-01', 'pandora_charm_bracelet.jpg', 0),
(11, 'Tiffany & Co. Engagement Ring', 'Timeless diamond ring for your special moment.', 199999, '2024-09-01', 'tiffany_engagement_ring.jpg', 0),
(11, 'TAG Heuer Monaco', 'Square-faced chronograph watch with a racing heritage.', 64999, '2024-10-01', 'tag_heuer_monaco.jpg', 1),

-- Luggage and Travel
(12, 'Samsonite Winfield 3 Luggage', 'Durable and stylish hardside suitcase.', 4999, '2024-06-01', 'samsonite_winfield_3.jpg', 1),
(12, 'Tumi Alpha 3 Carry-On', 'Premium carry-on luggage with smart features.', 8999, '2024-07-01', 'tumi_alpha_3_carryon.jpg', 0),
(12, 'Osprey Farpoint 40 Backpack', 'Lightweight and versatile travel backpack.', 2499, '2024-08-01', 'osprey_farpoint_40.jpg', 1),
(12, 'Away The Bigger Carry-On', 'Expandable carry-on luggage with a built-in charger.', 3999, '2024-09-01', 'away_bigger_carryon.jpg', 0),
(12, 'Delsey Paris Chatelet Luggage', 'Elegant luggage with a vintage design.', 5999, '2024-10-01', 'delsey_chatelet_luggage.jpg', 1),

-- Office and Stationery
(13, 'Apple MacBook Air M2', 'Thin and lightweight laptop with powerful M2 chip.', 14999, '2024-06-01', 'macbook_air_m2.jpg', 1),
(13, 'Montblanc Meisterstück Fountain Pen', 'Luxury fountain pen with a timeless design.', 7499, '2024-07-01', 'montblanc_meisterstuck.jpg', 0),
(13, 'Moleskine Classic Notebook', 'Iconic notebook for journaling and note-taking.', 1999, '2024-08-01', 'moleskine_classic_notebook.jpg', 1),
(13, 'Logitech MX Master 3S Mouse', 'Advanced ergonomic mouse for productivity.', 2999, '2024-09-01', 'logitech_mx_master_3s.jpg', 1),
(13, 'Ergotron WorkFit-TL Standing Desk Converter', 'Sit-stand desk converter for a healthier workspace.', 4999, '2024-10-01', 'ergotron_workfit_tl.jpg', 0),

-- Pet Products
(14, 'Furbo Dog Camera', 'Interactive camera that lets you see, talk, and toss treats to your pet.', 3999, '2024-06-01', 'furbo_dog_camera.jpg', 1),
(14, 'KONG Classic Dog Toy', 'Durable and bouncy toy for dogs of all sizes.', 999, '2024-07-01', 'kong_classic.jpg', 0),
(14, 'Whisker Litter-Robot', 'Self-cleaning litter box for cats.', 5999, '2024-08-01', 'whisker_litter_robot.jpg', 1),
(14, 'PetSafe Automatic Pet Feeder', 'Programmable feeder for consistent meals.', 2999, '2024-09-01', 'petsafe_automatic_feeder.jpg', 0),
(14, 'Seresto Flea and Tick Collar', 'Long-lasting flea and tick prevention for dogs.', 1999, '2024-10-01', 'seresto_flea_tick_collar.jpg', 1),

-- Sports and Outdoor
(15, 'Trek Domane SL 7', 'Performance road bike with endurance geometry.', 59999, '2024-06-01', 'trek_domane_sl7.jpg', 1),
(15, 'YETI Tundra 45 Cooler', 'Heavy-duty cooler with unmatched ice retention.', 3999, '2024-07-01', 'yeti_tundra_45.jpg', 0),
(15, 'Garmin Forerunner 955', 'Advanced GPS smartwatch for runners and triathletes.', 3999, '2024-08-01', 'garmin_forerunner_955.jpg', 1),
(15, 'REI Co-op Half Dome Tent', 'Versatile and durable tent for outdoor adventures.', 2499, '2024-09-01', 'rei_half_dome_tent.jpg', 0),
(15, 'Patagonia Nano Puff Jacket', 'Lightweight insulated jacket for cold weather.', 2499, '2024-10-01', 'patagonia_nano_puff.jpg', 1),

-- Tools and DIY
(16, 'DeWalt DCK1020D2 20V MAX Combo Kit', 'Complete set of tools for home improvement projects.', 7499, '2024-06-01', 'dewalt_dck1020d2.jpg', 0),
(16, 'Bosch GLL 3-80 Laser Level', '360-degree laser level for precise measurements.', 3999, '2024-07-01', 'bosch_gll_3_80.jpg', 1),
(16, 'Milwaukee M18 Fuel Cordless Drill', 'High-performance drill with a brushless motor.', 2999, '2024-08-01', 'milwaukee_m18_drill.jpg', 0),
(16, 'Makita 18V LXT Circular Saw', 'Powerful cordless saw for cutting wood and other materials.', 4999, '2024-09-01', 'makita_lxt_circular_saw.jpg', 1),
(16, 'Kreg K5 Pocket-Hole Jig', 'Versatile pocket-hole jig for woodworking projects.', 1499, '2024-10-01', 'kreg_k5_jig.jpg', 0),

-- Toys and Games
(17, 'LEGO Creator Expert Roller Coaster', 'Intricate LEGO set with a fully functional roller coaster.', 3999, '2024-06-01', 'lego_creator_roller_coaster.jpg', 1),
(17, 'PlayStation 5 Console', 'Next-gen gaming console with ultra-fast SSD.', 4999, '2024-07-01', 'playstation_5.jpg', 1),
(17, 'NERF Ultra One Blaster', 'High-capacity motorized blaster for intense battles.', 999, '2024-08-01', 'nerf_ultra_one.jpg', 0),
(17, 'Ravensburger Disney Villainous Board Game', 'Strategy board game where you play as Disney villains.', 499, '2024-09-01', 'ravensburger_disney_villainous.jpg', 1),
(17, 'Barbie Dreamhouse', 'Three-story dollhouse with furniture and accessories.', 4999, '2024-10-01', 'barbie_dreamhouse.jpg', 0);
