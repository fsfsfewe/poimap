const express = require('express');
const cors = require('cors');
const { Pool } = require('pg');

const app = express();

// Middleware: Cho phép Frontend nhận/gửi dữ liệu và xử lý file ảnh lớn (Base64)
app.use(cors()); 
app.use(express.json({ limit: '50mb' })); 
app.use(express.urlencoded({ limit: '50mb', extended: true }));

// CẤU HÌNH KẾT NỐI DATABASE POSTGRESQL
const pool = new Pool({
  user: 'postgres',        
  host: 'localhost',
  database: 'foodtour_db', 
  password: '123456',      // <--- Đảm bảo đúng pass của bạn
  port: 5432,
});

pool.connect((err) => {
  if (err) console.error('❌ Lỗi kết nối Database:', err.stack);
  else console.log('✅ Đã kết nối thành công tới PostgreSQL!');
});

// ==========================================
// 🍔 1. QUẢN LÝ QUÁN ĂN (EATERIES)
// ==========================================

// Lấy danh sách tất cả quán ăn
app.get('/api/eateries', async (req, res) => {
  try {
    const result = await pool.query('SELECT * FROM eateries ORDER BY id ASC');
    
    res.json({
      success: true,
      data: result.rows
    });
  } catch (err) {
    console.error(err.message);
    res.status(500).json({ success: false, error: "Lỗi máy chủ" });
  }
});

// Thêm quán ăn mới (Vendor)
app.post('/api/eateries', async (req, res) => {
  try {
    const { name, address, latitude, longitude, description, logo, main_image, gallery } = req.body;
    const newEatery = await pool.query(
      `INSERT INTO eateries (name, address, latitude, longitude, description, logo, main_image, gallery) 
       VALUES ($1, $2, $3, $4, $5, $6, $7, $8) RETURNING *`,
      [name, address, latitude, longitude, description, logo, main_image, JSON.stringify(gallery || [])]
    );
    res.json({ success: true, message: 'Đã lưu thành công!', data: newEatery.rows[0] });
  } catch (err) {
    res.status(500).json({ success: false, message: "Lỗi thêm quán ăn!" });
  }
});

// Cập nhật thông tin quán (Sửa)
app.put('/api/eateries/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const { name, address, latitude, longitude, description, logo, main_image, gallery } = req.body;
    const updateEatery = await pool.query(
      `UPDATE eateries 
       SET name = $1, address = $2, latitude = $3, longitude = $4, description = $5, logo = $6, main_image = $7, gallery = $8
       WHERE eatery_id = $9 RETURNING *`,
      [name, address, latitude, longitude, description, logo, main_image, JSON.stringify(gallery || []), id]
    );
    res.json({ success: true, message: 'Cập nhật thành công!', data: updateEatery.rows[0] });
  } catch (err) {
    res.status(500).json({ success: false, message: "Lỗi cập nhật quán ăn!" });
  }
});

// Xóa một quán ăn
app.delete('/api/eateries/:id', async (req, res) => {
  try {
    const { id } = req.params;
    await pool.query("DELETE FROM eateries WHERE eatery_id = $1", [id]);
    res.json({ success: true, message: "Đã xóa quán ăn thành công!" });
  } catch (err) {
    res.status(500).json({ success: false, message: "Lỗi máy chủ khi xóa!" });
  }
});


// ==========================================
// 👥 2. QUẢN LÝ NGƯỜI DÙNG (USERS)
// ==========================================

// Lấy danh sách người dùng (Cho trang Admin)
app.get('/api/users', async (req, res) => {
  try {
    const allUsers = await pool.query("SELECT * FROM users ORDER BY created_at DESC");
    res.json({ success: true, data: allUsers.rows });
  } catch (err) {
    res.status(500).json({ success: false, message: err.message });
  }
});

// Tạo người dùng mới (Register / Admin thêm)
app.post('/api/users', async (req, res) => {
  try {
    const { name, email, password, role } = req.body;
    
    // 1. Chuẩn hóa Role 
    const formattedRole = role ? (role.charAt(0).toUpperCase() + role.slice(1).toLowerCase()) : 'User';

   
    const query = `
      INSERT INTO users (name, email, username, password, role, status) 
      VALUES ($1, $2, $2, $3, $4, 'Hoạt động') 
      RETURNING *
    `;
    const values = [name, email, password || '123456', formattedRole];

    const newUser = await pool.query(query, values);

    console.log("✅ Đã tạo User thành công:", newUser.rows[0].email);
    res.json({ 
      success: true, 
      message: 'Tạo tài khoản thành công!', 
      data: newUser.rows[0] 
    });

  } catch (err) {
  
    console.error("❌ LỖI DATABASE:", err.message);
    
    res.status(500).json({ 
      success: false, 
      message: "Lỗi hệ thống: " + err.message 
    });
  }
});
// Khóa / Mở khóa tài khoản
app.put('/api/users/:id/status', async (req, res) => {
  try {
    const { id } = req.params;
    const { status } = req.body; // 'Hoạt động' hoặc 'Khóa'
    const updateUser = await pool.query(
      "UPDATE users SET status = $1 WHERE id = $2 RETURNING *",
      [status, id]
    );
    res.json({ success: true, message: 'Cập nhật trạng thái thành công!', data: updateUser.rows[0] });
  } catch (err) {
    res.status(500).json({ success: false, message: "Lỗi cập nhật người dùng!" });
  }
});



const PORT = 5000;
app.listen(PORT, () => {
  console.log(`🚀 Server Backend FoodTour đang chạy tại: http://localhost:${PORT}`);
});