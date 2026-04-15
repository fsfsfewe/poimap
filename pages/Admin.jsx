import React, { useState, useEffect } from 'react';
import axios from 'axios';
import CustomMap from '../components/CustomMap';
import { 
  ShieldIcon, UtensilsIcon, LogOutIcon, StoreIcon, MapPinIcon, 
  UsersIcon, BarChartIcon, EyeIcon, TrashIcon, XIcon, SafeImage, PlusIcon 
} from '../components/Icons';

const API_EATERIES = 'http://localhost:5000/api/eateries';
const API_USERS = 'http://localhost:5000/api/users';

export default function Admin({ onLogout }) {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [data, setData] = useState([]);
  const [users, setUsers] = useState([]); // <--- Dữ liệu trống ban đầu, sẽ fetch từ DB
  
  const [viewingEatery, setViewingEatery] = useState(null);
  const [isAddingUser, setIsAddingUser] = useState(false);
  const [newUser, setNewUser] = useState({ name: '', email: '', password: '', role: 'User' });

  // 1. KÉO DỮ LIỆU TỪ DATABASE
  const fetchData = async () => {
    try {
      const resEateries = await axios.get(API_EATERIES);
      if (resEateries.data.success) setData(resEateries.data.data);

      const resUsers = await axios.get(API_USERS);
      if (resUsers.data.success) setUsers(resUsers.data.data);
    } catch (e) {
      console.error("Lỗi kết nối API", e);
    }
  };

  useEffect(() => { fetchData(); }, []);

  // 2. XÓA QUÁN  
  const handleDeleteEatery = async (id) => {
    if(confirm("XÓA VĨNH VIỄN cơ sở này khỏi hệ thống?")) {
      await axios.delete(`${API_EATERIES}/${id}`);
      fetchData(); // Load lại danh sách sau khi xóa
    }
  };

  // 3. THÊM USER 
  const handleAddUser = async () => {
    if(!newUser.name || !newUser.email || !newUser.password) return alert("Vui lòng điền đủ thông tin!");
    try {
      await axios.post(API_USERS, newUser);
      alert("Đã tạo tài khoản thành công!");
      setIsAddingUser(false);
      setNewUser({ name: '', email: '', password: '', role: 'User' });
      fetchData(); // Load lại danh sách User
    } catch (error) {
      alert("Lỗi: Email này có thể đã tồn tại trong hệ thống!");
    }
  };

  // 4. KHÓA/MỞ KHÓA USER 
  const toggleUserStatus = async (id, currentStatus) => {
    const newStatus = currentStatus === 'Hoạt động' ? 'Khóa' : 'Hoạt động';
    try {
      await axios.put(`${API_USERS}/${id}/status`, { status: newStatus });
      fetchData();
    } catch (error) {
      alert("Lỗi khi cập nhật trạng thái!");
    }
  };

  // --- RENDER GIAO DIỆN ---
  

  const renderDashboard = () => (
    <div className="flex flex-col gap-8 h-full">
      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6 shrink-0">
        <div className="bg-white p-6 rounded-[2rem] shadow-sm border flex flex-col items-center text-center">
          <StoreIcon size={32} className="text-orange-500 mb-3" />
          <p className="text-slate-400 font-black uppercase text-[10px] tracking-widest">Tổng Quán Ăn</p>
          <span className="text-4xl font-black text-slate-800">{data.length}</span>
        </div>
        <div className="bg-white p-6 rounded-[2rem] shadow-sm border flex flex-col items-center text-center">
          <MapPinIcon size={32} className="text-blue-500 mb-3" />
          <p className="text-slate-400 font-black uppercase text-[10px] tracking-widest">Đã Ghim Vị Trí</p>
          <span className="text-4xl font-black text-slate-800">{data.filter(i => i.latitude).length}</span>
        </div>
        <div className="bg-white p-6 rounded-[2rem] shadow-sm border flex flex-col items-center text-center">
          <UsersIcon size={32} className="text-purple-500 mb-3" />
          <p className="text-slate-400 font-black uppercase text-[10px] tracking-widest">Tổng Người Dùng</p>
          <span className="text-4xl font-black text-slate-800">{users.length}</span>
        </div>
        <div className="bg-white p-6 rounded-[2rem] shadow-sm border flex flex-col items-center text-center">
          <ShieldIcon size={32} className="text-green-500 mb-3" />
          <p className="text-slate-400 font-black uppercase text-[10px] tracking-widest">Trạng Thái Server</p>
          <span className="text-2xl font-black text-green-500 mt-2">Online</span>
        </div>
      </div>
      <div className="flex-1 bg-white p-2 rounded-[3rem] shadow-lg border overflow-hidden relative">
        <CustomMap eateries={data} />
        <div className="absolute top-6 left-6 z-[1000] bg-white/90 backdrop-blur px-5 py-3 rounded-2xl shadow-sm border font-black text-[10px] uppercase tracking-widest flex items-center gap-2">
          <MapPinIcon className="text-orange-600" size={16}/> Bản đồ phân bổ
        </div>
      </div>
    </div>
  );

  const renderEateries = () => (
    <div className="bg-white rounded-[2.5rem] shadow-lg border overflow-hidden flex flex-col h-full">
      <div className="p-6 border-b bg-slate-50 flex justify-between items-center shrink-0">
        <h3 className="font-black text-slate-800 uppercase tracking-widest">Quản lý Dữ liệu Quán ăn</h3>
      </div>
      <div className="flex-1 overflow-y-auto custom-scrollbar">
        <table className="w-full text-left">
          <thead className="bg-slate-50 sticky top-0 border-b">
            <tr className="text-[10px] font-black text-slate-400 uppercase tracking-widest">
              <th className="px-6 py-4">ID / Logo</th><th className="px-6 py-4">Thông tin Quán</th><th className="px-6 py-4 text-center">Tọa độ GIS</th><th className="px-6 py-4 text-right">Quản trị</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {data.map(item => (
              <tr key={item.eatery_id} className="hover:bg-slate-50">
                <td className="px-6 py-4 flex gap-3 items-center">
                  <span className="text-xs font-mono text-slate-400">#{item.eatery_id.slice(0,4)}</span>
                  <SafeImage src={item.logo || item.image_url} className="w-10 h-10 rounded-xl object-cover shadow-sm shrink-0" />
                </td>
                <td className="px-6 py-4"><div className="font-bold text-slate-800">{item.name}</div><div className="text-[11px] text-slate-500 max-w-[250px] truncate">{item.address}</div></td>
                <td className="px-6 py-4 text-center"><span className="bg-slate-100 px-3 py-1 rounded-lg text-[10px] font-mono text-slate-600">{parseFloat(item.latitude).toFixed(3)}, {parseFloat(item.longitude).toFixed(3)}</span></td>
                <td className="px-6 py-4 text-right"><button onClick={() => setViewingEatery(item)} className="p-2 text-blue-500 hover:bg-blue-50 rounded-lg transition-all"><EyeIcon size={18}/></button><button onClick={() => handleDeleteEatery(item.eatery_id)} className="p-2 text-red-500 hover:bg-red-50 rounded-lg transition-all"><TrashIcon size={18}/></button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderUsers = () => (
    <div className="bg-white rounded-[2.5rem] shadow-lg border overflow-hidden flex flex-col h-full relative">
      <div className="p-6 border-b bg-slate-50 flex justify-between items-center shrink-0">
        <h3 className="font-black text-slate-800 uppercase tracking-widest">Tài khoản & Phân quyền</h3>
        <button onClick={() => setIsAddingUser(true)} className="bg-slate-900 text-white px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest flex items-center gap-2"><PlusIcon size={16}/> Thêm User</button>
      </div>
      
      {/* MODAL THÊM USER MỚI */}
      {isAddingUser && (
        <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-6">
          <div className="bg-white p-8 rounded-[2.5rem] w-full max-w-md shadow-2xl border">
            <h3 className="font-black text-xl mb-6 uppercase text-slate-800 text-center">Tạo Tài Khoản Mới</h3>
            <div className="space-y-4">
              <input className="w-full p-4 bg-slate-50 border rounded-2xl outline-none focus:ring-2 focus:ring-orange-500 font-bold" placeholder="Họ và tên..." value={newUser.name} onChange={e=>setNewUser({...newUser, name:e.target.value})} />
              <input className="w-full p-4 bg-slate-50 border rounded-2xl outline-none focus:ring-2 focus:ring-orange-500" placeholder="Email đăng nhập..." value={newUser.email} onChange={e=>setNewUser({...newUser, email:e.target.value})} />
              <input className="w-full p-4 bg-slate-50 border rounded-2xl outline-none focus:ring-2 focus:ring-orange-500" type="password" placeholder="Mật khẩu..." value={newUser.password} onChange={e=>setNewUser({...newUser, password:e.target.value})} />
              <select className="w-full p-4 bg-slate-50 border rounded-2xl outline-none focus:ring-2 focus:ring-orange-500 font-bold text-slate-600" value={newUser.role} onChange={e=>setNewUser({...newUser, role:e.target.value})}>
                <option value="User">Thực Khách (User)</option>
                <option value="Vendor">Chủ Quán (Vendor)</option>
                <option value="Admin">Quản Trị Viên (Admin)</option>
              </select>
              <div className="flex gap-3 pt-4"><button onClick={handleAddUser} className="flex-1 bg-orange-600 text-white py-4 rounded-2xl font-black uppercase shadow-lg hover:bg-orange-700">Tạo ngay</button><button onClick={() => setIsAddingUser(false)} className="px-6 bg-slate-100 text-slate-600 font-bold rounded-2xl hover:bg-slate-200">Hủy</button></div>
            </div>
          </div>
        </div>
      )}

      <div className="flex-1 overflow-y-auto custom-scrollbar">
        <table className="w-full text-left">
          <thead className="bg-slate-50 sticky top-0 border-b"><tr className="text-[10px] font-black text-slate-400 uppercase tracking-widest"><th className="px-6 py-4">Tài khoản</th><th className="px-6 py-4">Vai trò</th><th className="px-6 py-4 text-center">Trạng thái</th><th className="px-6 py-4 text-right">Hành động</th></tr></thead>
          <tbody className="divide-y divide-slate-100">
            {users.map(u => (
              <tr key={u.id} className="hover:bg-slate-50">
                <td className="px-6 py-4"><div className="font-bold text-slate-800">{u.name}</div><div className="text-[11px] text-slate-500">{u.email}</div></td>
                <td className="px-6 py-4"><span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase ${u.role === 'Admin' ? 'bg-purple-100 text-purple-700' : u.role === 'Vendor' ? 'bg-orange-100 text-orange-700' : 'bg-blue-100 text-blue-700'}`}>{u.role}</span></td>
                <td className="px-6 py-4 text-center"><span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase ${u.status === 'Hoạt động' ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}`}>{u.status}</span></td>
                <td className="px-6 py-4 text-right"><button onClick={() => toggleUserStatus(u.id, u.status)} className="px-4 py-2 bg-slate-100 text-slate-600 rounded-lg text-[10px] font-black uppercase hover:bg-slate-200">{u.status === 'Hoạt động' ? 'Khóa TK' : 'Mở Khóa'}</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );

  const renderAnalytics = () => (
    <div className="bg-white rounded-[2.5rem] shadow-lg border p-8 flex flex-col h-full overflow-y-auto custom-scrollbar">
      {/* Bảng KPI Trực quan */}
      <div className="grid grid-cols-3 gap-6 mb-10 pb-10 border-b">
        <div className="bg-orange-50 p-6 rounded-3xl flex items-center justify-between">
          <div><p className="text-slate-500 font-black text-[10px] uppercase tracking-widest mb-1">Lượt Truy Cập</p><p className="text-4xl font-black text-slate-800">49.2K</p></div><BarChartIcon size={40} className="text-orange-300" />
        </div>
        <div className="bg-blue-50 p-6 rounded-3xl flex items-center justify-between">
          <div><p className="text-slate-500 font-black text-[10px] uppercase tracking-widest mb-1">Thành viên mới</p><p className="text-4xl font-black text-slate-800">+{users.length}</p></div><UsersIcon size={40} className="text-blue-300" />
        </div>
        <div className="bg-green-50 p-6 rounded-3xl flex items-center justify-between">
          <div><p className="text-slate-500 font-black text-[10px] uppercase tracking-widest mb-1">Cơ sở đăng ký</p><p className="text-4xl font-black text-green-600">{data.length}</p></div><div className="w-12 h-12 bg-green-200 rounded-full flex items-center justify-center text-green-600 font-black">📈</div>
        </div>
      </div>
      <h3 className="font-black text-slate-800 uppercase tracking-widest mb-8">Biểu Đồ Lưu Lượng 7 Ngày Qua</h3>
      <div className="flex-1 relative flex items-end justify-between min-h-[300px] pt-12 pb-6 px-4">
        {/* Lưới và Cột  */}
        <div className="absolute inset-0 flex flex-col justify-between pointer-events-none opacity-20 py-6"><div className="border-t border-dashed border-slate-400 w-full"></div><div className="border-t border-dashed border-slate-400 w-full"></div><div className="border-t border-dashed border-slate-400 w-full"></div><div className="border-t border-solid border-slate-500 w-full"></div></div>
        {[35, 65, 45, 90, 70, 85, 100].map((val, idx) => (
          <div key={idx} className="relative flex-1 flex flex-col items-center justify-end h-full group">
            <div className="w-1/2 max-w-[60px] bg-orange-100 rounded-t-2xl relative overflow-hidden transition-all duration-300 flex items-end" style={{ height: `${val}%` }}>
              <div className="w-full bg-gradient-to-t from-orange-600 to-orange-400 group-hover:from-orange-500 transition-all duration-500" style={{ height: '100%' }}></div>
            </div>
            <span className="text-[11px] font-black text-slate-400 mt-4 group-hover:text-orange-600 transition-colors">T{idx + 2}</span>
          </div>
        ))}
      </div>
    </div>
  );

  return (
    <div className="flex h-screen w-screen bg-slate-100 font-sans overflow-hidden">
      
      {/* MODAL XEM CHI TIẾT QUÁN */}
      {viewingEatery && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-sm z-[9999] flex items-center justify-center p-6">
          <div className="bg-white w-full max-w-3xl rounded-[3rem] shadow-2xl flex flex-col max-h-[90vh] overflow-hidden border border-white/20">
            <div className="p-6 border-b flex justify-between items-center bg-slate-50">
              <h3 className="font-black text-lg uppercase tracking-widest text-slate-800 flex items-center gap-2"><StoreIcon className="text-orange-500"/> Hồ sơ cơ sở</h3>
              <button onClick={() => setViewingEatery(null)} className="p-2 bg-slate-200 rounded-full hover:bg-red-100 hover:text-red-500 transition-all"><XIcon size={20}/></button>
            </div>
            <div className="p-8 overflow-y-auto custom-scrollbar flex-1 space-y-8">
              <div className="flex gap-6 items-center">
                <SafeImage src={viewingEatery.logo || viewingEatery.image_url} className="w-24 h-24 rounded-3xl object-cover border-4 border-slate-100 shadow-md shrink-0" />
                <div><h2 className="text-3xl font-black text-slate-900">{viewingEatery.name}</h2><p className="text-slate-500 mt-1 font-bold text-sm bg-slate-100 inline-block px-3 py-1 rounded-lg mt-2">{viewingEatery.address}</p></div>
              </div>
              <div className="bg-slate-50 p-6 rounded-3xl border"><p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Giới thiệu</p><p className="text-sm leading-relaxed text-slate-700">{viewingEatery.description || 'Chưa cập nhật bài viết.'}</p></div>
              <div className="grid grid-cols-2 gap-6">
                <div><p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Ảnh Bìa</p><SafeImage src={viewingEatery.main_image || viewingEatery.image_url} className="w-full h-40 object-cover rounded-3xl border shadow-sm" /></div>
                <div>
                  <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Tọa độ Vệ Tinh (GPS)</p>
                  <div className="h-40 bg-slate-900 rounded-3xl flex flex-col items-center justify-center font-mono font-bold text-green-400 shadow-inner">
                    <span>LAT: {parseFloat(viewingEatery.latitude).toFixed(5)}</span><span>LNG: {parseFloat(viewingEatery.longitude).toFixed(5)}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* SIDEBAR BÊN TRÁI */}
      <aside className="w-72 bg-[#020617] text-white flex flex-col p-6 shrink-0 shadow-2xl z-20">
        <div className="mb-10 flex items-center gap-3 px-2">
          <div className="bg-orange-600 p-2.5 rounded-xl shadow-lg"><ShieldIcon size={24}/></div>
          <span className="text-2xl font-black italic uppercase tracking-tighter">Core<span className="text-orange-500">Admin</span></span>
        </div>
        <nav className="flex-1 space-y-3">
          <button onClick={() => setActiveTab('dashboard')} className={`w-full flex items-center gap-4 px-5 py-4 rounded-2xl font-bold uppercase text-[11px] tracking-widest transition-all ${activeTab === 'dashboard' ? 'bg-orange-600 text-white shadow-lg' : 'text-slate-400 hover:bg-white/5 hover:text-white'}`}><UtensilsIcon size={18} /> Tổng quan</button>
          <button onClick={() => setActiveTab('eateries')} className={`w-full flex items-center gap-4 px-5 py-4 rounded-2xl font-bold uppercase text-[11px] tracking-widest transition-all ${activeTab === 'eateries' ? 'bg-orange-600 text-white shadow-lg' : 'text-slate-400 hover:bg-white/5 hover:text-white'}`}><StoreIcon size={18} /> Quản lý Quán</button>
          <button onClick={() => setActiveTab('users')} className={`w-full flex items-center gap-4 px-5 py-4 rounded-2xl font-bold uppercase text-[11px] tracking-widest transition-all ${activeTab === 'users' ? 'bg-orange-600 text-white shadow-lg' : 'text-slate-400 hover:bg-white/5 hover:text-white'}`}><UsersIcon size={18} /> Người dùng</button>
          <button onClick={() => setActiveTab('analytics')} className={`w-full flex items-center gap-4 px-5 py-4 rounded-2xl font-bold uppercase text-[11px] tracking-widest transition-all ${activeTab === 'analytics' ? 'bg-orange-600 text-white shadow-lg' : 'text-slate-400 hover:bg-white/5 hover:text-white'}`}><BarChartIcon size={18} /> Lưu lượng</button>
        </nav>
        <button onClick={onLogout} className="mt-auto flex items-center justify-center gap-2 p-4 rounded-2xl bg-white/5 text-slate-400 font-black hover:text-red-500 transition-all uppercase text-[10px] tracking-widest"><LogOutIcon size={18}/> Đăng xuất</button>
      </aside>

      <main className="flex-1 p-8 overflow-hidden bg-slate-50">
        {activeTab === 'dashboard' && renderDashboard()}
        {activeTab === 'eateries' && renderEateries()}
        {activeTab === 'users' && renderUsers()}
        {activeTab === 'analytics' && renderAnalytics()}
      </main>
    </div>
  );
}