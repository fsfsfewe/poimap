import React, { useState, useEffect } from 'react';
import axios from 'axios';
import CustomMap from '../components/CustomMap';
import { 
  StoreIcon, LogOutIcon, EditIcon, TrashIcon, CameraIcon, 
  MapPinIcon, ImageIcon, UploadIcon, XIcon, SafeImage 
} from '../components/Icons';


const API_URL = 'http://localhost:5000/api/eateries';

export default function Vendor({ onLogout }) {
  const [data, setData] = useState([]);
  const [pos, setPos] = useState(null);
  const [editingId, setEditingId] = useState(null);

  // Form chứa Ảnh thực tế
  const [form, setForm] = useState({
    name: '', address: '', description: '', 
    logo: null, main_image: null, gallery: []
  });

  const fetchEateries = async () => {
    try { const res = await axios.get(API_URL); if (res.data.success) setData(res.data.data); } catch (e) {}
  };

  useEffect(() => { fetchEateries(); }, []);

  // HÀM XỬ LÝ CHỌN FILE TỪ MÁY TÍNH
  const handleFileChange = (e, field) => {
    const files = Array.from(e.target.files);
    if (!files.length) return;

    if (field === 'gallery') {
      const newImages = files.map(f => URL.createObjectURL(f));
      setForm(prev => ({ ...prev, gallery: [...prev.gallery, ...newImages] }));
    } else {
      setForm(prev => ({ ...prev, [field]: URL.createObjectURL(files[0]) }));
    }
  };

  const removeGalleryImage = (index) => {
    setForm(prev => ({ ...prev, gallery: prev.gallery.filter((_, i) => i !== index) }));
  };

  const handleSave = async (e) => {
    e.preventDefault();
    if (!pos) return alert("Vui lòng ghim vị trí quán trên bản đồ!");
    
    const payload = { ...form, latitude: pos.lat, longitude: pos.lng };
    try {
      if (editingId) await axios.put(`${API_URL}/${editingId}`, payload);
      else await axios.post(API_URL, payload);
      
      alert("Đã lưu thông tin quán!");
      resetForm(); fetchEateries();
    } catch (e) { alert("Lỗi lưu dữ liệu!"); }
  };

  const resetForm = () => {
    setForm({ name: '', address: '', description: '', logo: null, main_image: null, gallery: [] });
    setPos(null); setEditingId(null);
  };

  return (
    <div className="flex flex-col h-screen w-screen bg-slate-100 font-sans overflow-hidden">
      {/* Header */}
      <header className="bg-white px-8 py-4 flex justify-between items-center shadow-sm shrink-0">
        <div className="flex items-center gap-3">
          <div className="bg-orange-600 p-2 rounded-xl text-white"><StoreIcon /></div>
          <h2 className="text-xl font-black text-slate-800">Cổng Đối Tác (Vendor)</h2>
        </div>
        <button onClick={onLogout} className="flex items-center gap-2 font-bold text-slate-500 hover:text-red-500"><LogOutIcon size={18}/> Thoát</button>
      </header>

      {/* Main Content */}
      <main className="flex-1 flex gap-6 p-6 overflow-hidden">
        
        {/* Form Nhập Liệu (Bên Trái) */}
        <div className="w-[500px] bg-white rounded-3xl shadow-lg border p-8 flex flex-col overflow-y-auto custom-scrollbar shrink-0">
          <h3 className="text-xl font-black mb-6 uppercase text-slate-800">{editingId ? "Cập nhật Cơ Sở" : "Thêm Cơ Sở Mới"}</h3>
          
          <form onSubmit={handleSave} className="space-y-6 flex-1">
            {/* Upload Logo & Tên */}
            <div className="flex items-start gap-4">
              <label className="shrink-0 cursor-pointer">
                <div className="w-20 h-20 bg-slate-50 border-2 border-dashed rounded-2xl flex flex-col items-center justify-center overflow-hidden hover:border-orange-400">
                  {form.logo ? <img src={form.logo} className="w-full h-full object-cover"/> : <CameraIcon className="text-slate-400"/>}
                </div>
                <input type="file" hidden accept="image/*" onChange={(e) => handleFileChange(e, 'logo')} />
              </label>
              <div className="flex-1 space-y-3">
                <input required className="w-full px-4 py-3 bg-slate-50 border rounded-xl font-bold focus:ring-2 focus:ring-orange-500 outline-none" placeholder="Tên quán..." value={form.name} onChange={e=>setForm({...form, name:e.target.value})} />
                <input required className="w-full px-4 py-3 bg-slate-50 border rounded-xl focus:ring-2 focus:ring-orange-500 outline-none text-sm" placeholder="Địa chỉ chi tiết..." value={form.address} onChange={e=>setForm({...form, address:e.target.value})} />
              </div>
            </div>

            {/* Mô tả */}
            <textarea className="w-full px-4 py-3 bg-slate-50 border rounded-xl focus:ring-2 focus:ring-orange-500 outline-none min-h-[100px] text-sm" placeholder="Mô tả món ăn, không gian quán..." value={form.description} onChange={e=>setForm({...form, description:e.target.value})} />

            {/* Ảnh Chính & Album */}
            <div className="grid grid-cols-2 gap-4">
              <label className="cursor-pointer">
                <div className="p-4 bg-slate-50 border-2 border-dashed rounded-xl flex flex-col items-center justify-center hover:border-orange-400">
                  <ImageIcon className="text-orange-500 mb-1" />
                  <span className="text-[10px] font-black uppercase">Ảnh Bìa</span>
                </div>
                <input type="file" hidden accept="image/*" onChange={(e) => handleFileChange(e, 'main_image')} />
              </label>
              
              <label className="cursor-pointer">
                <div className="p-4 bg-slate-50 border-2 border-dashed rounded-xl flex flex-col items-center justify-center hover:border-blue-400">
                  <UploadIcon className="text-blue-500 mb-1" />
                  <span className="text-[10px] font-black uppercase">Album Ảnh</span>
                </div>
                <input type="file" hidden multiple accept="image/*" onChange={(e) => handleFileChange(e, 'gallery')} />
              </label>
            </div>

            {/* Hiển thị Album Mini */}
            {form.gallery.length > 0 && (
              <div className="flex gap-2 overflow-x-auto py-2">
                {form.gallery.map((img, idx) => (
                  <div key={idx} className="relative w-16 h-16 shrink-0">
                    <img src={img} className="w-full h-full rounded-lg object-cover border shadow-sm" />
                    <button type="button" onClick={()=>removeGalleryImage(idx)} className="absolute -top-1 -right-1 bg-red-500 text-white rounded-full p-0.5"><XIcon size={12}/></button>
                  </div>
                ))}
              </div>
            )}

            {/* Tọa độ */}
            <div className={`p-4 rounded-xl border text-xs font-bold flex items-center gap-3 ${pos ? 'bg-green-50 text-green-700' : 'bg-orange-50 text-orange-600'}`}>
              <MapPinIcon size={18} />
              {pos ? `Tọa độ: ${pos.lat.toFixed(4)}, ${pos.lng.toFixed(4)}` : "Click bản đồ để ghim vị trí"}
            </div>

            <button className="w-full bg-slate-900 text-white py-4 rounded-2xl font-black uppercase tracking-widest hover:bg-orange-600 transition-all">{editingId ? "Cập nhật" : "Lưu hệ thống"}</button>
            {editingId && <button type="button" onClick={resetForm} className="w-full py-2 text-slate-400 font-bold text-sm">Hủy</button>}
          </form>
        </div>

        {/* Bản đồ & Danh sách (Bên Phải) */}
        <div className="flex-1 flex flex-col gap-6 overflow-hidden">
          {/* Map Container - Bọc cẩn thận để không vỡ CSS */}
          <div className="h-[40%] bg-white rounded-3xl border shadow-lg overflow-hidden relative p-1">
             <CustomMap eateries={data} onMapClick={setPos} selectedPos={pos} />
          </div>

          {/* List Quán */}
          <div className="flex-1 bg-white rounded-3xl shadow-lg border overflow-hidden flex flex-col">
            <div className="p-5 border-b bg-slate-50"><h3 className="font-black text-slate-800 uppercase tracking-widest">Danh sách cơ sở ({data.length})</h3></div>
            <div className="flex-1 overflow-y-auto">
              <table className="w-full text-left">
                <thead className="bg-slate-50 sticky top-0"><tr className="text-[10px] font-black text-slate-400 uppercase"><th className="p-4">Quán</th><th className="p-4 text-right">Quản lý</th></tr></thead>
                <tbody className="divide-y">
                  {data.map(item => (
                    <tr key={item.eatery_id} className="hover:bg-slate-50">
                      <td className="p-4 flex gap-4 items-center">
                        <SafeImage src={item.logo || item.image_url} className="w-12 h-12 rounded-xl object-cover border-2 border-white shadow-sm shrink-0" />
                        <div><p className="font-black text-sm">{item.name}</p><p className="text-xs text-slate-500">{item.address}</p></div>
                      </td>
                      <td className="p-4 text-right">
                        <button onClick={()=>{
                          setEditingId(item.eatery_id);
                          setForm({ name: item.name, address: item.address, description: item.description||'', logo: item.logo||null, main_image: item.main_image||null, gallery: item.gallery||[] });
                          setPos({ lat: parseFloat(item.latitude), lng: parseFloat(item.longitude) });
                        }} className="text-blue-600 font-bold mr-4"><EditIcon/></button>
                        <button onClick={async ()=>{if(confirm("Xóa?")){await axios.delete(`${API_URL}/${item.eatery_id}`); fetchEateries();}}} className="text-red-600 font-bold"><TrashIcon/></button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

      </main>
    </div>
  );
}