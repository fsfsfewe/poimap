-- 1. Kích hoạt extension để tự động tạo ID ngẫu nhiên và bảo mật (UUID)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 2. Tạo kiểu dữ liệu Enum cho phân quyền người dùng
CREATE TYPE user_role AS ENUM ('USER', 'ADMIN', 'RESTAURANT_OWNER');

-- 3. Tạo bảng Users (Người dùng)
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role user_role DEFAULT 'USER',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. Tạo bảng Eateries (Danh sách quán ăn)
CREATE TABLE eateries (
    eatery_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    description TEXT,
    address TEXT NOT NULL,
    latitude DECIMAL(10, 8), -- Tọa độ vĩ độ (để gắn lên bản đồ)
    longitude DECIMAL(11, 8), -- Tọa độ kinh độ
    category VARCHAR(100), -- VD: 'Ăn vặt', 'Nhậu', 'Hải sản'
    price_min INT,
    price_max INT,
    average_rating DECIMAL(3, 2) DEFAULT 0.0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tạo bảng Menu_Items (Thực đơn của quán)
CREATE TABLE menu_items (
    item_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    eatery_id UUID REFERENCES eateries(eatery_id) ON DELETE CASCADE,
    name VARCHAR(255) NOT NULL,
    price INT NOT NULL,
    image_url TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 6. Tạo bảng Reviews (Đánh giá quán ăn)
CREATE TABLE reviews (
    review_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(user_id) ON DELETE CASCADE,
    eatery_id UUID REFERENCES eateries(eatery_id) ON DELETE CASCADE,
    rating INT CHECK (rating >= 1 AND rating <= 5),
    comment TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 7. Tạo bảng Tours (Lịch trình Foodtour)
CREATE TABLE tours (
    tour_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID REFERENCES users(user_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    total_estimated_cost INT DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 8. Tạo bảng Tour_Stops (Các trạm dừng trong 1 Foodtour)
CREATE TABLE tour_stops (
    stop_id SERIAL PRIMARY KEY,
    tour_id UUID REFERENCES tours(tour_id) ON DELETE CASCADE,
    eatery_id UUID REFERENCES eateries(eatery_id) ON DELETE CASCADE,
    stop_order INT NOT NULL, -- Thứ tự điểm đến (1, 2, 3...)
    UNIQUE (tour_id, stop_order) -- Đảm bảo trong 1 tour không có 2 trạm trùng số thứ tự
);

-- 9. TẠO INDEX (Phần ăn điểm cho đồ án)
CREATE INDEX idx_eateries_name_address ON eateries (name, address);
CREATE INDEX idx_reviews_eatery ON reviews (eatery_id);