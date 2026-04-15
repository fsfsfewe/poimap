const { Pool } = require('pg');
const fs = require('fs');

const pool = new Pool({
  user: 'postgres',
  host: 'localhost',
  database: 'foodtour_db',
  password: '123456', 
  port: 5432,
});

async function importData() {
  try {
    // Kiểm tra file có tồn tại không
    if (!fs.existsSync('export.json')) {
      console.error("❌ Không tìm thấy file export.json! Ngân hãy copy nó vào thư mục này nhé.");
      return;
    }

    const rawData = fs.readFileSync('export.json'); 
    const data = JSON.parse(rawData);

    if (!data.elements || data.elements.length === 0) {
      console.error("❌ File JSON không có dữ liệu (elements trống)!");
      return;
    }

    console.log(`🔍 Tìm thấy tổng cộng ${data.elements.length} đối tượng trong file.`);
    let count = 0;

    for (const item of data.elements) {
      // Chỉ lấy nếu có tên và có tọa độ
      if (item.tags && item.tags.name && item.lat && item.lon) {
        const name = item.tags.name;
        const address = item.tags['addr:street'] || item.tags['addr:full'] || 'Quận 7, TP.HCM';
        const lat = item.lat;
        const lon = item.lon;

        try {
          await pool.query(
            "INSERT INTO eateries (name, address, latitude, longitude, description) VALUES ($1, $2, $3, $4, $5)",
            [name, address, lat, lon, 'Dữ liệu thật từ OpenStreetMap']
          );
          count++;
          console.log(`✅ Đã nạp: ${name}`);
        } catch (dbErr) {
          // Nếu trùng tên hoặc lỗi cột, nó sẽ báo ở đây
          console.warn(`⚠️ Bỏ qua quán "${name}": ${dbErr.message}`);
        }
      }
    }

    console.log(`\n🎉 HOÀN THÀNH! Đã nạp thành công ${count} quán vào Database.`);
    process.exit();
  } catch (err) {
    console.error("❌ LỖI NGHIÊM TRỌNG:", err.message);
    process.exit(1);
  }
}

importData();