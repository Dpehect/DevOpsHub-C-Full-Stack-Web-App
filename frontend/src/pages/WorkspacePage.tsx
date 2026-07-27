import { FormEvent, useEffect, useMemo, useState } from 'react';
import { Building2, ChevronRight, Crown, MailPlus, Plus, Search, ShieldCheck, Trash2, Users } from 'lucide-react';
import { request } from '../api/client';
import type { WorkspaceDetails, WorkspaceRole, WorkspaceSummary } from '../workspaces/types';

const roles: WorkspaceRole[] = ['Viewer','Member','Manager','Admin'];

export function WorkspacePage({ onBack }: { onBack: () => void }) {
  const [items,setItems]=useState<WorkspaceSummary[]>([]); const [selected,setSelected]=useState<WorkspaceDetails|null>(null);
  const [query,setQuery]=useState(''); const [busy,setBusy]=useState(false); const [error,setError]=useState('');
  const [createOpen,setCreateOpen]=useState(false); const [inviteOpen,setInviteOpen]=useState(false);
  async function load(){ setItems(await request<WorkspaceSummary[]>('/api/workspaces')); }
  async function open(id:string){ setSelected(await request<WorkspaceDetails>(`/api/workspaces/${id}`)); }
  useEffect(()=>{load().catch(e=>setError(e.message))},[]);
  const filtered=useMemo(()=>items.filter(x=>`${x.name} ${x.slug}`.toLowerCase().includes(query.toLowerCase())),[items,query]);
  async function create(e:FormEvent<HTMLFormElement>){ e.preventDefault(); setBusy(true); const f=new FormData(e.currentTarget); try { const w=await request<WorkspaceSummary>('/api/workspaces',{method:'POST',body:JSON.stringify({name:f.get('name'),slug:f.get('slug'),description:f.get('description')})}); setCreateOpen(false); await load(); await open(w.id); } catch(err){setError((err as Error).message)} finally{setBusy(false)} }
  async function invite(e:FormEvent<HTMLFormElement>){ e.preventDefault(); if(!selected)return; setBusy(true); const f=new FormData(e.currentTarget); try{await request(`/api/workspaces/${selected.workspace.id}/invitations`,{method:'POST',body:JSON.stringify({email:f.get('email'),role:f.get('role')})});setInviteOpen(false);await open(selected.workspace.id)}catch(err){setError((err as Error).message)}finally{setBusy(false)} }
  async function remove(memberId:string){ if(!selected)return; await request(`/api/workspaces/${selected.workspace.id}/members/${memberId}`,{method:'DELETE'}); await open(selected.workspace.id); }
  return <div className="workspace-shell">
    <header className="topbar"><button className="ghost" onClick={onBack}>← Dashboard</button><div><b>Workspace Center</b><span>Takım, roller ve erişim yönetimi</span></div><button className="primary" onClick={()=>setCreateOpen(true)}><Plus size={16}/> Yeni workspace</button></header>
    {error&&<div className="error-banner">{error}<button onClick={()=>setError('')}>×</button></div>}
    <main className="workspace-layout">
      <aside className="workspace-list-panel"><div className="search"><Search size={16}/><input placeholder="Workspace ara" value={query} onChange={e=>setQuery(e.target.value)}/></div>
      <div className="workspace-list">{filtered.map(w=><button key={w.id} className={`workspace-row ${selected?.workspace.id===w.id?'active':''}`} onClick={()=>open(w.id)}><div className="workspace-icon"><Building2 size={18}/></div><div><strong>{w.name}</strong><span>{w.memberCount} üye · {w.currentUserRole}</span></div><ChevronRight size={16}/></button>)}</div></aside>
      <section className="workspace-detail">{selected?<>
        <div className="detail-hero"><div><span className="eyebrow">/{selected.workspace.slug}</span><h1>{selected.workspace.name}</h1><p>{selected.workspace.description||'Takımınız için merkezi çalışma alanı.'}</p></div><div className="hero-stat"><Users/><strong>{selected.members.length}</strong><span>aktif üye</span></div></div>
        <div className="section-head"><div><h2>Takım üyeleri</h2><p>Workspace erişimi ve yetki seviyeleri</p></div>{['Owner','Admin'].includes(selected.workspace.currentUserRole)&&<button className="secondary" onClick={()=>setInviteOpen(true)}><MailPlus size={16}/> Üye davet et</button>}</div>
        <div className="member-table"><div className="table-head"><span>Kullanıcı</span><span>Rol</span><span>Katılım</span><span></span></div>{selected.members.map(m=><div className="member-row" key={m.id}><div className="member-user"><div className="avatar">{m.displayName.slice(0,2).toUpperCase()}</div><div><strong>{m.displayName}</strong><span>{m.email}</span></div></div><span className={`role role-${m.role.toLowerCase()}`}>{m.role==='Owner'?<Crown size={13}/>:<ShieldCheck size={13}/>} {m.role}</span><span>{new Date(m.joinedAt).toLocaleDateString('tr-TR')}</span><button className="icon-danger" disabled={m.role==='Owner'} onClick={()=>remove(m.id)}><Trash2 size={15}/></button></div>)}</div>
        {selected.invitations.length>0&&<><div className="section-head compact"><div><h2>Bekleyen davetler</h2><p>7 gün içinde geçerliliğini yitirir</p></div></div><div className="invite-grid">{selected.invitations.map(i=><div className="invite-card" key={i.id}><MailPlus/><div><strong>{i.email}</strong><span>{i.role} · {new Date(i.expiresAt).toLocaleDateString('tr-TR')}</span></div></div>)}</div></>}
      </>:<div className="empty-state"><Building2 size={42}/><h2>Bir workspace seç</h2><p>Takım detaylarını, üyeleri ve davetleri burada yöneteceksin.</p></div>}</section>
    </main>
    {createOpen&&<div className="modal-backdrop"><form className="modal" onSubmit={create}><h2>Yeni workspace</h2><label>Ad<input name="name" required maxLength={100}/></label><label>Slug<input name="slug" required pattern="[a-z0-9-]+" placeholder="platform-ekibi"/></label><label>Açıklama<textarea name="description" rows={3}/></label><div className="modal-actions"><button type="button" className="ghost" onClick={()=>setCreateOpen(false)}>İptal</button><button className="primary" disabled={busy}>Oluştur</button></div></form></div>}
    {inviteOpen&&<div className="modal-backdrop"><form className="modal" onSubmit={invite}><h2>Takıma üye ekle</h2><label>E-posta<input name="email" type="email" required/></label><label>Rol<select name="role">{roles.map(r=><option key={r}>{r}</option>)}</select></label><div className="modal-actions"><button type="button" className="ghost" onClick={()=>setInviteOpen(false)}>İptal</button><button className="primary" disabled={busy}>Davet et</button></div></form></div>}
  </div>
}
