import React, { useEffect, useRef } from 'react';

/**
 * HÀM TẠO ICON TÙY CHỈNH
 * @param {string} type: 'user', 'tour', hoặc 'default'
 * @param {number} number: Số thứ tự trong lộ trình
 */
const createMarkerIcon = (type, number = null) => {
  let html = '';
  let iconSize = [30, 30];
  let iconAnchor = [15, 15];

  if (type === 'user') {
    // Icon Vị trí của tôi: Chấm xanh có sóng lan tỏa
    html = `
      <div class="relative">
        <div class="w-4 h-4 bg-blue-500 rounded-full border-2 border-white shadow-lg z-10 relative"></div>
        <div class="absolute top-0 left-0 w-4 h-4 bg-blue-400 rounded-full animate-ping opacity-75"></div>
      </div>
    `;
    iconSize = [16, 16];
    iconAnchor = [8, 8];
  } else if (type === 'tour') {
    // Icon quán trong Tour: Màu cam đậm, có số trắng nổi bật
    html = `
      <div class="flex items-center justify-center w-8 h-8 bg-orange-600 rounded-full border-2 border-white shadow-xl text-white font-bold text-sm">
        ${number}
      </div>
    `;
    iconSize = [32, 32];
    iconAnchor = [16, 16];
  } else {
    // Icon quán chưa chọn: Chấm tròn xám mờ nhỏ gọn
    html = `
      <div class="w-3 h-3 bg-slate-400 rounded-full border border-white opacity-60"></div>
    `;
    iconSize = [12, 12];
    iconAnchor = [6, 6];
  }

  return window.L.divIcon({
    html: html,
    className: 'custom-div-icon',
    iconSize: iconSize,
    iconAnchor: iconAnchor
  });
};

export default function CustomMap({ eateries = [], userLoc, tourRoute = [] }) {
  const mapRef = useRef(null);
  const mapInstance = useRef(null);
  const markerLayer = useRef(null);
  const routeLayer = useRef(null);

  // 1. KHỞI TẠO BẢN ĐỒ BAN ĐẦU
  useEffect(() => {
    if (!mapRef.current || mapInstance.current) return;

    // Tọa độ mặc định tại TP.HCM
    const map = window.L.map(mapRef.current).setView([10.7626, 106.6601], 13);
    
    window.L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(map);

    markerLayer.current = window.L.layerGroup().addTo(map);
    routeLayer.current = window.L.layerGroup().addTo(map);
    mapInstance.current = map;

    // Sửa lỗi bản đồ bị xám khi render
    setTimeout(() => map.invalidateSize(), 500);
  }, []);

  // 2. VẼ MARKERS VÀ LỘ TRÌNH THỰC TẾ
  useEffect(() => {
    if (!window.L || !mapInstance.current) return;

    // Xóa trắng để vẽ lại khi dữ liệu thay đổi
    markerLayer.current.clearLayers();
    routeLayer.current.clearLayers();

    // A. Vẽ vị trí của người dùng
    if (userLoc && userLoc.lat && userLoc.lng) {
      window.L.marker([userLoc.lat, userLoc.lng], {
        icon: createMarkerIcon('user'),
        zIndexOffset: 2000 // Luôn hiện trên cùng
      }).addTo(markerLayer.current);
    }

    // B. Vẽ tất cả các quán ăn
    eateries.forEach((e) => {
      // Logic quan trọng: So sánh ID (ép kiểu String) để xác định quán trong Tour
      const tourIndex = tourRoute.findIndex(item => String(item.id) === String(e.id));
      const isInTour = tourIndex !== -1;

      const marker = window.L.marker([e.latitude, e.longitude], {
        icon: isInTour ? createMarkerIcon('tour', tourIndex + 1) : createMarkerIcon('default'),
        zIndexOffset: isInTour ? 1000 : 0 // Quán được chọn sẽ nằm đè lên quán chưa chọn
      });

      marker.bindPopup(`
        <div class="p-1">
          <strong class="text-orange-600">${e.name}</strong><br/>
          <small class="text-gray-500">${e.address || ''}</small>
          ${isInTour ? `<p class="mt-1 font-bold">📍 Điểm dừng số ${tourIndex + 1}</p>` : ''}
        </div>
      `);
      marker.addTo(markerLayer.current);
    });

    // C. Gọi API OSRM để vẽ đường uốn lượn theo tuyến đường
    if (tourRoute.length > 1) {
      // Tạo chuỗi tọa độ (lng,lat) cho API
      const coords = tourRoute.map(p => `${p.lng},${p.lat}`).join(';');
      const osrmUrl = `https://router.project-osrm.org/route/v1/driving/${coords}?overview=full&geometries=geojson`;

      fetch(osrmUrl)
        .then(res => res.json())
        .then(data => {
          if (data.routes && data.routes.length > 0) {
            const geometry = data.routes[0].geometry;
            const latlngs = geometry.coordinates.map(c => [c[1], c[0]]);

            // Vẽ bóng đổ nhạt phía dưới đường chính
            window.L.polyline(latlngs, {
              color: '#000', weight: 8, opacity: 0.1
            }).addTo(routeLayer.current);

            // Vẽ đường chính màu cam rực rỡ
            const polyline = window.L.polyline(latlngs, {
              color: '#ea580c',
              weight: 5,
              opacity: 0.9,
              lineJoin: 'round'
            }).addTo(routeLayer.current);

            // Tự động căn chỉnh màn hình để thấy hết đường đi
            mapInstance.current.fitBounds(polyline.getBounds(), { padding: [50, 50] });
          }
        })
        .catch(err => console.error("Lỗi Routing OSRM:", err));
    }
  }, [eateries, userLoc, tourRoute]);

  return (
    <div className="w-full h-full relative overflow-hidden bg-slate-100 shadow-inner">
      <div ref={mapRef} className="w-full h-full z-0" />
      
      {/* Nút hỗ trợ Zoom nhanh nếu cần */}
      <div className="absolute bottom-4 right-4 z-[1000] flex flex-col gap-2">
         {userLoc && (
           <button 
             onClick={() => mapInstance.current.flyTo([userLoc.lat, userLoc.lng], 16)}
             className="p-2 bg-white rounded-full shadow-lg border border-gray-200 hover:bg-gray-50"
             title="Về vị trí của tôi"
           >
             🎯
           </button>
         )}
      </div>
    </div>
  );
}