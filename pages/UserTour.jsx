import React, { useState, useEffect } from 'react';
import axios from 'axios';
import CustomMap from '../components/CustomMap';
import { LogOutIcon, NavigationIcon, RouteIcon, MapPinIcon, SafeImage, XIcon } from '../components/Icons';

const API_URL = 'http://localhost:5000/api/eateries';

export default function UserTour({ onLogout }) {
  const [eateries, setEateries] = useState([]);
  const [userLoc, setUserLoc] = useState(null);
  const [tourList, setTourList] = useState([]);

  useEffect(() => { axios.get(API_URL)
  .then(res => {
    // Lấy dữ liệu bất kể Backend trả về mảng hay object
    const data = Array.isArray(res.data) ? res.data : res.data.data;
    if (data) {
      setEateries(data);
    }
  })
  .catch(err => console.error("Lỗi tải dữ liệu:", err));
  }, []);

  // Lấy định vị GPS thực tế
  const getMyLocation = () => {
  if (!navigator.geolocation) {
    return alert("Trình duyệt của bạn không hỗ trợ định vị GPS!");
  }

  
  const options = {
    enableHighAccuracy: true, 
    timeout: 10000,           
    maximumAge: 0             // Luôn lấy vị trí mới nhất
  };

  // Hiển thị một log nhỏ để Ngân dễ theo dõi trong Console (F12)
  console.log("Đang quét tìm vị trí của Ngân...");

  navigator.geolocation.getCurrentPosition(
    (pos) => {
      const { latitude, longitude } = pos.coords;
      
    
      if (typeof latitude === 'number' && typeof longitude === 'number') {
        console.log(`✅ Lấy vị trí thành công: ${latitude}, ${longitude}`);
        setUserLoc({ lat: latitude, lng: longitude });
      }
    },
    (err) => {
     
      let errorMessage = "";
      switch(err.code) {
        case err.PERMISSION_DENIED:
          errorMessage = "Ngân ơi, bạn cần 'Cho phép' (Allow) quyền truy cập vị trí trên trình duyệt nhé!";
          break;
        case err.POSITION_UNAVAILABLE:
          errorMessage = "Không tìm thấy tín hiệu GPS. Ngân thử kiểm tra lại kết nối mạng nha.";
          break;
        case err.TIMEOUT:
          errorMessage = "Lấy vị trí quá lâu (hết 10s). Ngân hãy thử nhấn lại lần nữa xem sao.";
          break;
        default:
          errorMessage = "Có lỗi lạ khi lấy vị trí, Ngân kiểm tra Console (F12) thử nhé.";
      }
      alert(errorMessage);
      console.error("Lỗi GPS:", err);
    },
    options
  );
};

  // Thêm quán vào lộ trình
 const addToTour = (eatery) => {
  setTourList(prevList => {
    const isExisted = prevList.some(item => String(item.id) === String(eatery.id));
    if (!isExisted) {
      return [...prevList, eatery];
    }
    alert("Quán này đã có trong lộ trình!");
    return prevList;
  });
};

  // Tính toán Tọa độ 
 const tourRoute = [];
if (userLoc) tourRoute.push({ lat: userLoc.lat, lng: userLoc.lng });

tourList.forEach(t => {
  const lat = parseFloat(t.latitude);
  const lng = parseFloat(t.longitude);
  if (!isNaN(lat) && !isNaN(lng)) {
    tourRoute.push({ lat, lng });
  }
});
  return (
    <div className="flex flex-col lg:flex-row h-screen w-screen bg-slate-50 overflow-hidden font-sans">
      {/* BẢNG ĐIỀU KHIỂN BÊN TRÁI */}
      <div className="w-full lg:w-[450px] bg-white shadow-2xl z-10 flex flex-col shrink-0 border-r">
        <div className="p-6 bg-slate-900 text-white flex justify-between items-center">
          <div><h2 className="text-2xl font-black italic text-orange-500">FOOD<span className="text-white">TOUR</span></h2><p className="text-[10px] uppercase tracking-widest text-slate-400">Khám phá ẩm thực</p></div>
          <button onClick={onLogout} className="text-slate-400 hover:text-white"><LogOutIcon size={24}/></button>
        </div>

        <div className="p-6 border-b">
          <button onClick={getMyLocation} className="w-full bg-blue-50 text-blue-600 py-3 rounded-2xl font-black flex items-center justify-center gap-2 hover:bg-blue-600 hover:text-white transition-all uppercase tracking-widest text-xs border border-blue-200">
            <NavigationIcon size={18} /> Lấy vị trí của tôi
          </button>
        </div>

        {/* Lộ trình Food Tour */}
        <div className="flex-1 overflow-y-auto p-6 bg-slate-50 space-y-6">
          <div>
            <h3 className="font-black text-slate-800 uppercase tracking-widest text-sm flex items-center gap-2 mb-4"><RouteIcon className="text-orange-500"/> Lộ trình của bạn ({tourList.length})</h3>
            {tourList.length === 0 ? (
              <div className="text-center p-6 border-2 border-dashed rounded-2xl text-slate-400 text-xs font-bold uppercase">Chưa có quán nào trong lộ trình</div>
            ) : (
              <div className="space-y-3">
                {tourList.map((t, idx) => (
                  <div key={t.eatery_id} className="flex items-center gap-3 bg-white p-3 rounded-2xl shadow-sm border border-slate-100">
                    <div className="w-6 h-6 rounded-full bg-orange-100 text-orange-600 flex items-center justify-center text-xs font-black shrink-0">{idx + 1}</div>
                    <div className="flex-1 truncate"><p className="font-bold text-sm text-slate-800 truncate">{t.name}</p></div>
                    <button onClick={()=>setTourList(tourList.filter(x => x.eatery_id !== t.eatery_id))} className="text-red-400 hover:text-red-600 p-1"><XIcon size={16}/></button>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Gợi ý quán ăn */}
          <div>
            <h3 className="font-black text-slate-800 uppercase tracking-widest text-sm mb-4">Các điểm đến nổi bật</h3>
            <div className="space-y-4">
              {eateries.map(e => (
                <div key={e.eatery_id} className="bg-white p-3 rounded-2xl shadow-sm border border-slate-100 flex gap-4 items-center cursor-pointer hover:border-orange-300 transition-all" onClick={()=>addToTour(e)}>
                  <SafeImage src={e.logo || e.image_url} className="w-16 h-16 rounded-xl object-cover" />
                  <div className="flex-1 overflow-hidden">
                    <p className="font-bold text-slate-800 text-sm truncate">{e.name}</p>
                    <p className="text-xs text-slate-500 truncate mb-2">{e.address}</p>
                    <span className="text-[10px] font-black bg-orange-50 text-orange-600 px-2 py-1 rounded-lg uppercase">+ Thêm vào Tour</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>

      {/* BẢN ĐỒ BÊN PHẢI */}
      <div className="flex-1 relative h-full">
        <CustomMap eateries={eateries} userLoc={userLoc} tourRoute={tourRoute} />
        {/* Hướng dẫn góc trên */}
        <div className="absolute top-6 left-6 z-[1000] bg-white/90 backdrop-blur px-5 py-4 rounded-2xl shadow-xl border font-bold text-xs text-slate-700 max-w-xs leading-relaxed">
          📍 Bấm <span className="text-blue-600 font-black">Lấy vị trí</span> để hiện chấm xanh.<br/> 
          🍔 Chọn quán để nối bản đồ <span className="text-orange-500 font-black">Lộ trình đi ăn</span>.
        </div>
      </div>
    </div>
  );
}