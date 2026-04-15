using System.Collections.Generic;

namespace poimap
{
    public static class LanguageHelper
    {
        // Danh sách từ vựng Tiếng Anh
        private static readonly Dictionary<string, string> EnglishDict = new Dictionary<string, string>
        {
            // Màn hình quét QR
            { "BẬT CAMERA QUÉT", "OPEN CAMERA" },
            { "DÙNG ẢNH CÓ SẴN", "PICK FROM GALLERY" },
            { "Scan QR to open Audio Guide", "Scan QR to open Audio Guide" },
            
            // Màn hình Bản đồ (MainActivity)
            { "Tìm kiếm quán ăn...", "Search places..." },
            { "Tất cả", "All" },
            
            // Màn hình Tour
            { "Lộ trình tham quan", "Audio Tours" },
            { "Khám phá Vĩnh Khánh theo lộ trình của bạn", "Discover Vinh Khanh your way" },
            { "Lộ trình nổi bật", "Featured Tours" },
            { "Tất cả lộ trình", "All Tours" },
            
            // Màn hình Profile
            { "LỊCH SỬ ĐÁNH GIÁ CỦA BẠN", "YOUR REVIEW HISTORY" },
            { "Khách hàng Ẩn danh", "Anonymous Guest" },


            // --- CÁC TỪ MỚI CHO CÁC TRANG CÒN LẠI ---
// Chi tiết Tour
{ "DANH SÁCH ĐỊA ĐIỂM TRONG TOUR", "LIST OF PLACES IN TOUR" },
{ "Không có dữ liệu. Vui lòng kiểm tra lại tên Tour trên Firebase!", "No data. Please check Tour name on Firebase!" },

// Đánh giá Quán
{ "Đánh giá Quán Ăn", "Review Place" },
{ "Chia sẻ trải nghiệm của bạn về quán này...", "Share your experience about this place..." },
{ "LƯU ĐÁNH GIÁ", "SUBMIT REVIEW" },
{ "Vui lòng chọn số sao!", "Please select a rating!" },
{ "Cảm ơn bạn đã đánh giá!", "Thank you for your review!" },
{ "Lỗi kết nối mạng", "Network error" },

// Nghe Audio
{ "Khám phá Địa điểm", "Explore Location" },
{ "Ngôn ngữ: ", "Language: " },
{ "Đã tải xong Audio, sẵn sàng phát!", "Audio loaded, ready to play!" },
{ "Lỗi tải Audio", "Error loading Audio" } ,

// Thông báo quét QR
{ "Mở khóa thành công! Chào mừng bạn.", "Unlock successful! Welcome." },
{ "Mã QR không hợp lệ! Vui lòng thử lại.", "Invalid QR code! Please try again." },
{ "Không tìm thấy mã QR nào trong ảnh!", "No QR code found in the image!" },

// Thanh menu dưới đáy (Bottom Navigation)
{ "Bản đồ", "Map" },
{ "Tour ăn uống", "Food Tours" },
{ "Cá nhân", "Profile" }



        };

        // Hàm dịch: Đưa vào tiếng Việt -> Trả ra tiếng Anh (nếu đang chọn English)
        public static string GetText(string vnText)
        {
            if (IntroActivity.CurrentLang == "vi")
                return vnText; // Nếu đang tiếng Việt thì giữ nguyên chữ gốc

            // Nếu đang tiếng Anh, tìm trong từ điển, nếu không thấy thì trả về chữ gốc
            return EnglishDict.ContainsKey(vnText) ? EnglishDict[vnText] : vnText;
        }
    }
}