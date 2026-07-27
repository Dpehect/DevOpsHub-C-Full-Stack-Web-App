import { useEffect, useMemo, useState } from 'react';
import { Activity, AlertTriangle, ArrowLeft, Clock3, FileSearch, RefreshCw, Search, ShieldCheck } from 'lucide-react';
import { getSession } from '../api/client';
import type { AuditEntry, ObservabilityStats, PagedResult, SystemLog } from '../observability/types';

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api';
async function get<T>(path:string):Promise<T>{ const token=getSession()?.accessToken; const r=await fetch(`${API_URL}${path}`,{headers:{Authorization:`Bearer ${token}`}}); if(!r.ok) throw new Error('Veri alınamadı'); return r.json(); }

export default function ObservabilityPage({onBack}:{onBack:()=>void}){
  const [tab,setTab]=useState<'logs'|'audit'>('logs');
  const [logs,setLogs]=useState<SystemLog[]>([]); const [audits,setAudits]=useState<AuditEntry[]>([]);
  const [stats,setStats]=useState<ObservabilityStats>({logs24h:0,errors24h:0,audits24h:0,averageRequestMs:0,errorRatePercent:0});
  const [search,setSearch]=useState(''); const [level,setLevel]=useState(''); const [loading,setLoading]=useState(true);
  async function load(){ setLoading(true); try { const [s,l,a]=await Promise.all([get<ObservabilityStats>('/observability/stats'),get<PagedResult<SystemLog>>('/observability/logs?pageSize=100'),get<PagedResult<AuditEntry>>('/observability/audit?pageSize=100')]); setStats(s);setLogs(l.items);setAudits(a.items);} finally{setLoading(false)} }
  useEffect(()=>{void load()},[]);
  const filteredLogs=useMemo(()=>logs.filter(x=>(!level||x.level===level)&&(!search||`${x.message} ${x.path??''} ${x.requestId??''}`.toLowerCase().includes(search.toLowerCase()))),[logs,level,search]);
  const filteredAudits=useMemo(()=>audits.filter(x=>!search||`${x.action} ${x.entityType} ${x.userEmail??''} ${x.requestId??''}`.toLowerCase().includes(search.toLowerCase())),[audits,search]);
  return <main className="observability-page">
    <header className="obs-header"><button className="secondary" onClick={onBack}><ArrowLeft size={16}/> Dashboard</button><div><p className="eyebrow">Platform observability</p><h1>Logs & Audit Trail</h1><p className="muted">HTTP istekleri, hatalar ve kritik değişiklikler tek merkezde.</p></div><button className="primary" onClick={()=>void load()}><RefreshCw size={16}/> Yenile</button></header>
    <section className="obs-metrics">
      <article className="card"><Activity/><span>Son 24 saat log</span><strong>{stats.logs24h}</strong></article>
      <article className="card"><AlertTriangle/><span>Hata</span><strong>{stats.errors24h}</strong></article>
      <article className="card"><ShieldCheck/><span>Audit kayıtları</span><strong>{stats.audits24h}</strong></article>
      <article className="card"><Clock3/><span>Ort. istek</span><strong>{stats.averageRequestMs} ms</strong></article>
    </section>
    <section className="obs-panel">
      <div className="obs-toolbar"><div className="obs-tabs"><button className={tab==='logs'?'active':''} onClick={()=>setTab('logs')}>System logs</button><button className={tab==='audit'?'active':''} onClick={()=>setTab('audit')}>Audit trail</button></div><div className="obs-filters"><label><Search size={15}/><input value={search} onChange={e=>setSearch(e.target.value)} placeholder="Ara..."/></label>{tab==='logs'&&<select value={level} onChange={e=>setLevel(e.target.value)}><option value="">Tüm seviyeler</option><option>Information</option><option>Warning</option><option>Error</option><option>Critical</option></select>}</div></div>
      {loading?<div className="obs-empty">Yükleniyor...</div>:tab==='logs'?<div className="obs-table-wrap"><table className="obs-table"><thead><tr><th>Seviye</th><th>Mesaj</th><th>İstek</th><th>Süre</th><th>Zaman</th></tr></thead><tbody>{filteredLogs.map(x=><tr key={x.id}><td><span className={`log-level ${x.level.toLowerCase()}`}>{x.level}</span></td><td><strong>{x.message}</strong><small>{x.category} · {x.requestId}</small></td><td><code>{x.method} {x.path}</code><small>Status {x.statusCode}</small></td><td>{x.durationMs??'-'} ms</td><td>{new Date(x.createdAtUtc).toLocaleString('tr-TR')}</td></tr>)}</tbody></table></div>:<div className="obs-table-wrap"><table className="obs-table"><thead><tr><th>Sonuç</th><th>Eylem</th><th>Varlık</th><th>Kullanıcı</th><th>Zaman</th></tr></thead><tbody>{filteredAudits.map(x=><tr key={x.id}><td><span className={x.succeeded?'audit-ok':'audit-fail'}>{x.succeeded?'Başarılı':'Başarısız'}</span></td><td><strong>{x.action}</strong><small>{x.requestId}</small></td><td>{x.entityType}<small>{x.entityId}</small></td><td>{x.userEmail??'Anonymous'}<small>{x.ipAddress}</small></td><td>{new Date(x.createdAtUtc).toLocaleString('tr-TR')}</td></tr>)}</tbody></table></div>}
      {!loading&&((tab==='logs'&&filteredLogs.length===0)||(tab==='audit'&&filteredAudits.length===0))&&<div className="obs-empty"><FileSearch/> Kayıt bulunamadı.</div>}
    </section>
  </main>
}
