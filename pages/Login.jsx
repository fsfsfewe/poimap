import React, { useState } from 'react';
import axios from 'axios';
import { UtensilsIcon, UserIcon, ShieldIcon } from '../components/Icons';

export default function Login({ onLogin }) {
  const [isRegister, setIsRegister] = useState(false);
  const [form, setForm] = useState({ name: '', email: '', pass: '', role: 'User' }); // User, Vendor, Admin

  const submit = async (e) => {
    e.preventDefault();

    if (isRegister) {
      // THỰC HIỆN ĐĂNG KÝ VÀO DATABASE
      try {
        await axios.post('http://localhost:5000/api/users', { 
          name: form.name, 
          email: form.email, 
          password: form.pass, 
          role: form.role 
        });
        alert(`Đã đăng ký thành công! Vui lòng Đăng nhập.`);
        setIsRegister(false); 
      } catch (err) {
        alert("Lỗi: Email này có thể đã được sử dụng!");
      }
    } else {
      // THỰC HIỆN ĐĂNG NHẬP BẰNG CÁCH KIỂM TRA DATABASE
      try {
        const res = await axios.get('http://localhost:5000/api/users');
        const users = res.data.data;
        
        // Tìm xem có user nào trùng Email và Mật khẩu không
        const validUser = users.find(u => u.email === form.email && u.password === form.pass);

        if (validUser) {
          // Kiểm tra xem tài khoản có bị Admin khóa không
          if (validUser.status === 'Khóa') {
            return alert("Tài khoản của bạn đã bị khóa bởi Quản trị viên!");
          }
          
          alert(`Đăng nhập thành công với quyền: ${validUser.role}`);
          // Chuyển trang dựa trên Role (admin, vendor, user)
          onLogin(validUser.role.toLowerCase()); 
        } 
        // Lệnh Backup cho Admin hệ thống nếu lỡ xóa nhầm tài khoản Admin trong DB
        else if (form.email === 'admin' && form.pass === 'admin') {
          onLogin('admin');
        } 
        else {
          alert("Sai thông tin Đăng nhập (Email hoặc Mật khẩu)!");
        }
      } catch (error) {
        alert("Không thể kết nối tới Server Database!");
      }
    }
  };

  return (
    <div className="h-screen w-screen bg-[#020617] flex items-center justify-center p-6 relative overflow-hidden font-sans text-left">
      <div className="absolute w-[60vw] h-[60vh] bg-orange-600/10 rounded-full blur-[120px] top-0 left-0"></div>
      
      <div className="relative z-10 w-full max-w-[450px]">
        <div className="text-center mb-8 flex flex-col items-center">
          <div className="p-4 bg-gradient-to-br from-orange-500 to-red-600 rounded-3xl mb-4 shadow-xl"><UtensilsIcon size={48} className="text-white" /></div>
          <h1 className="text-4xl font-black text-white italic tracking-tighter">FOOD<span className="text-orange-500 not-italic">TOUR</span></h1>
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest mt-2">Nền tảng ẩm thực số 1</p>
        </div>

        <form onSubmit={submit} className="bg-white/5 backdrop-blur-2xl border border-white/10 p-10 rounded-[3rem] shadow-2xl space-y-6">
          <h2 className="text-xl font-black text-white text-center uppercase tracking-widest mb-6">
            {isRegister ? 'Đăng Ký Tài Khoản' : 'Đăng Nhập'}
          </h2>

          {/* Ô nhập Tên (Chỉ hiện khi Đăng ký) */}
          {isRegister && (
            <div className="relative flex items-center">
              <UserIcon className="absolute left-4 text-slate-400"/>
              <input required className="w-full pl-12 pr-4 py-4 bg-white/5 rounded-2xl text-white outline-none focus:ring-2 focus:ring-orange-500" value={form.name} onChange={e=>setForm({...form, name:e.target.value})} placeholder="Họ và tên..."/>
            </div>
          )}

          {/* Ô nhập Email */}
          <div className="relative flex items-center">
            <UserIcon className="absolute left-4 text-slate-400"/>
            <input required type="text" className="w-full pl-12 pr-4 py-4 bg-white/5 rounded-2xl text-white outline-none focus:ring-2 focus:ring-orange-500" value={form.email} onChange={e=>setForm({...form, email:e.target.value})} placeholder="Email (admin, vendor, user)..."/>
          </div>

          {/* Ô nhập Mật khẩu */}
          <div className="relative flex items-center">
            <ShieldIcon className="absolute left-4 text-slate-400"/>
            <input required type="password" className="w-full pl-12 pr-4 py-4 bg-white/5 rounded-2xl text-white outline-none focus:ring-2 focus:ring-orange-500" value={form.pass} onChange={e=>setForm({...form, pass:e.target.value})} placeholder="Mật khẩu..."/>
          </div>
          
          {/* Ô chọn Role (Chỉ hiện khi Đăng ký) */}
          {isRegister && (
            <select className="w-full px-4 py-4 bg-slate-800 text-white border border-white/10 rounded-2xl outline-none focus:ring-2 focus:ring-orange-500 font-bold" value={form.role} onChange={e=>setForm({...form, role:e.target.value})}>
              <option value="User">Tôi là Thực khách (User)</option>
              <option value="Vendor">Tôi là Chủ quán (Vendor)</option>
            </select>
          )}

          <button className="w-full bg-orange-600 text-white font-black py-4 rounded-2xl shadow-lg hover:bg-orange-700 transition-all uppercase tracking-widest mt-2">{isRegister ? 'Tạo Tài Khoản' : 'Vào Hệ Thống'}</button>
        </form>

        <div className="mt-8 text-center">
          <button type="button" onClick={() => { setIsRegister(!isRegister); setForm({ name:'', email:'', pass:'', role:'User' }); }} className="text-slate-400 text-sm font-bold hover:text-white transition-all">
            {isRegister ? 'Đã có tài khoản? Đăng nhập ngay' : 'Chưa có tài khoản? Đăng ký ngay'}
          </button>
        </div>
      </div>
    </div>
  );
}