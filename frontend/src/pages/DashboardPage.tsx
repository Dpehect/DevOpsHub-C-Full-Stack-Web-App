import { Activity, Bell, GitBranch, LogOut, Server, ShieldCheck, Workflow, GripVertical, Eye, EyeOff, RotateCcw, Settings2, Clock3, Users, Gauge } from 'lucide-react';
import { useMemo, useState } from 'react';
import type { AuthResponse } from '../auth/types';

type WidgetId = 'projects'|'services'|'pipelines'|'incidents'|'deployments'|'team'|'latency'|'activity';
type Widget = { id: WidgetId; title: string; value: string; detail: string; icon: typeof Activity; tone: string };
const catalog: Widget[] = [
  { id:'projects', title:'Active projects', value:'12', detail:'+2 this month', icon:GitBranch, tone:'blue' },
  { id:'services', title:'Healthy services', value:'24/26', detail:'92.3% uptime', icon:Server, tone:'green' },
  { id:'pipelines', title:'Pipeline success', value:'94.8%', detail:'+3.4% in 30 days', icon:Activity, tone:'violet' },
  { id:'incidents', title:'Open incidents', value:'3', detail:'1 critical', icon:ShieldCheck, tone:'red' },
  { id:'deployments', title:'Deployments', value:'48', detail:'7 days', icon:Clock3, tone:'amber' },
  { id:'team', title:'Team capacity', value:'78%', detail:'14 active members', icon:Users, tone:'cyan' },
  { id:'latency', title:'API latency', value:'184 ms', detail:'p95 response time', icon:Gauge, tone:'pink' },
  { id:'activity', title:'Events today', value:'1,284', detail:'Across 9 services', icon:Bell, tone:'indigo' },
];
const defaultOrder: WidgetId[] = ['projects','services','pipelines','incidents','deployments','team','latency','activity'];
const read = <T,>(key:string, fallback:T):T => { try { const value=localStorage.getItem(key); return value ? JSON.parse(value) as T : fallback; } catch { return fallback; } };

type Props = { onOpenSearch: () => void; onOpenFiles:()=>void; onOpenCalendar:()=>void; onOpenExecutive:()=>void; onOpenWorkflow:()=>void; session: AuthResponse; onOpenAdmin: () => void; onLogout: () => void; onOpenWorkspaces: () => void; onOpenProjects: () => void; onOpenRepositories: () => void; onOpenPipelines: () => void; onOpenIncidents: () => void; onOpenObservability: () => void; onOpenDocumentation: () => void; onOpenAnalytics: () => void };
export function DashboardPage({ onOpenSearch, onOpenFiles, onOpenCalendar, onOpenExecutive, onOpenWorkflow, session, onLogout, onOpenWorkspaces, onOpenProjects, onOpenRepositories, onOpenPipelines, onOpenIncidents, onOpenObservability, onOpenDocumentation, onOpenAnalytics, onOpenAdmin }: Props) {
  const [order,setOrder]=useState<WidgetId[]>(()=>read('dashboard-order',defaultOrder));
  const [hidden,setHidden]=useState<WidgetId[]>(()=>read('dashboard-hidden',[]));
  const [editing,setEditing]=useState(false);
  const [dragged,setDragged]=useState<WidgetId|null>(null);
  const widgets=useMemo(()=>order.map(id=>catalog.find(w=>w.id===id)!).filter(w=>!hidden.includes(w.id)),[order,hidden]);
  const persist=(nextOrder=order,nextHidden=hidden)=>{ localStorage.setItem('dashboard-order',JSON.stringify(nextOrder)); localStorage.setItem('dashboard-hidden',JSON.stringify(nextHidden)); };
  const drop=(target:WidgetId)=>{ if(!dragged||dragged===target)return; const next=[...order]; const from=next.indexOf(dragged),to=next.indexOf(target); next.splice(from,1); next.splice(to,0,dragged); setOrder(next); persist(next,hidden); setDragged(null); };
  const toggle=(id:WidgetId)=>{ const next=hidden.includes(id)?hidden.filter(x=>x!==id):[...hidden,id]; setHidden(next); persist(order,next); };
  const reset=()=>{ setOrder(defaultOrder); setHidden([]); persist(defaultOrder,[]); };
  return <main className="shell">
    <aside className="sidebar"><div className="brand"><Workflow size={20}/>DevOpsHub</div><nav><a className="active">Overview</a><a onClick={onOpenSearch}>Global search</a><a onClick={onOpenWorkspaces}>Workspaces</a><a onClick={onOpenProjects}>Projects</a><a onClick={onOpenPipelines}>Pipelines</a><a onClick={onOpenIncidents}>Incidents</a><a onClick={onOpenRepositories}>Repositories</a><a onClick={onOpenObservability}>Logs & audit</a><a onClick={onOpenDocumentation}>Documentation</a><a onClick={onOpenAnalytics}>Analytics</a>{session.user.role === 'Admin' && <a onClick={onOpenAdmin}>Admin console</a>}</nav><div className="sidebar-user"><div className="avatar">{session.user.displayName.slice(0,2).toUpperCase()}</div><div><strong>{session.user.displayName}</strong><span>{session.user.role}</span></div></div></aside>
    <section className="content">
      <header><div><p className="eyebrow">Engineering workspace</p><h1>Operational overview</h1></div><div className="header-actions"><button className="icon-button"><Bell size={18}/></button><button className="secondary" onClick={()=>setEditing(!editing)}><Settings2 size={16}/>{editing?'Finish':'Customize'}</button><button className="secondary" onClick={onLogout}><LogOut size={16}/> Sign out</button><button className="primary" onClick={onOpenWorkspaces}>Manage workspace</button></div></header>
      {editing && <section className="widget-config panel"><div><p className="eyebrow">Dashboard layout</p><h2>Visible widgets</h2></div><div className="widget-toggle-list">{catalog.map(w=><button key={w.id} className={hidden.includes(w.id)?'widget-toggle is-hidden':'widget-toggle'} onClick={()=>toggle(w.id)}>{hidden.includes(w.id)?<EyeOff size={15}/>:<Eye size={15}/>} {w.title}</button>)}</div><button className="secondary" onClick={reset}><RotateCcw size={15}/> Reset layout</button></section>}
      <section className="grid dashboard-widgets">{widgets.map(({id,title,value,detail,icon:Icon,tone}) => <article draggable={editing} onDragStart={()=>setDragged(id)} onDragOver={e=>e.preventDefault()} onDrop={()=>drop(id)} className={`card dashboard-widget tone-${tone} ${editing?'editable':''}`} key={id}>{editing&&<GripVertical className="drag-handle" size={18}/>}<div className="metric-top"><Icon size={20}/><span>{title}</span></div><strong>{value}</strong><small>{detail}</small><div className="sparkline"><i/><i/><i/><i/><i/><i/><i/></div></article>)}</section>
      <section className="dashboard-grid"><article className="panel large"><div><p className="eyebrow">System status</p><h2>All core services operational</h2><p className="muted">Last checked less than a minute ago</p><div className="service-bars"><span style={{width:'96%'}}/><span style={{width:'89%'}}/><span style={{width:'93%'}}/></div></div><span className="status">Healthy</span></article><article className="panel"><p className="eyebrow">Security</p><h2>Session protected</h2><p className="muted">JWT access token with rotating refresh token enabled.</p><div className="security-score">96<span>/100</span></div></article></section>
      <section className="panel activity-panel"><div><p className="eyebrow">Live engineering feed</p><h2>Recent activity</h2></div><div className="activity-list"><span><b>Production deployment completed</b><small>API Gateway · 4 min ago</small></span><span><b>Pull request #284 approved</b><small>Billing Service · 12 min ago</small></span><span><b>Incident DEV-91 resolved</b><small>Authentication · 38 min ago</small></span><span><b>Sprint velocity updated</b><small>Platform Team · 1 hour ago</small></span></div></section>
    </section>
  
<section className="premium-launcher"><button onClick={onOpenFiles}><b>Files</b><span>Assets & versions</span></button><button onClick={onOpenCalendar}><b>Planner</b><span>Sprints & timeline</span></button><button onClick={onOpenExecutive}><b>Executive</b><span>Engineering KPIs</span></button><button onClick={onOpenWorkflow}><b>Approvals</b><span>Policy workflows</span></button></section>
</main>;
}
