import { useEffect, useState } from 'react';
import type { CSSProperties } from 'react';
import { ArrowLeft, Activity, AlertTriangle, BarChart3, Gauge, RefreshCw, TrendingUp } from 'lucide-react';
import { request } from '../api/client';
import type { AnalyticsOverview } from '../analytics';

export default function AnalyticsPage({onBack}:{onBack:()=>void}) {
  const [data,setData]=useState<AnalyticsOverview|null>(null); const [loading,setLoading]=useState(true);
  const load=()=>{setLoading(true);request<AnalyticsOverview>('/analytics/overview').then(setData).finally(()=>setLoading(false))};
  useEffect(load,[]);
  if(loading||!data) return <main className="analytics-page"><div className="analytics-loading"><RefreshCw className="spin"/> Loading engineering analytics…</div></main>;
  const max=Math.max(...data.deliveryTrend.map(x=>x.value));
  return <main className="analytics-page">
    <header className="analytics-header"><button className="secondary" onClick={onBack}><ArrowLeft size={16}/> Overview</button><div><p className="eyebrow">Engineering intelligence</p><h1>Analytics & health</h1><p className="muted">Delivery, reliability and operational risk in one view.</p></div><button className="secondary" onClick={load}><RefreshCw size={16}/> Refresh</button></header>
    <section className="analytics-hero">
      <article className="health-card"><div className="health-ring" style={{'--score':`${data.projectHealth.score*3.6}deg`} as CSSProperties}><div><strong>{data.projectHealth.score}</strong><span>/100</span></div></div><div><span className="health-grade">Grade {data.projectHealth.grade}</span><h2>{data.projectHealth.status}</h2><p className="muted">Weighted from deployment reliability, delivery completion, uptime and incident response.</p></div></article>
      <div className="analytics-metrics">{data.metrics.map(m=><article key={m.key}><div><span>{m.label}</span>{m.key==='success'?<Activity size={18}/>:m.key==='velocity'?<TrendingUp size={18}/>:m.key==='mttr'?<Gauge size={18}/>:<AlertTriangle size={18}/>}</div><strong>{m.value}</strong><small className={m.direction==='up'?'positive':'negative'}>{m.delta}</small></article>)}</div>
    </section>
    <section className="analytics-grid">
      <article className="analytics-panel span-2"><div className="panel-heading"><div><p className="eyebrow">Delivery trend</p><h2>Weekly throughput</h2></div><BarChart3/></div><div className="bar-chart">{data.deliveryTrend.map(x=><div className="bar-slot" key={x.label}><div className="bar-value">{x.value}</div><div className="bar" style={{height:`${Math.max(18,x.value/max*170)}px`}}/><span>{x.label}</span></div>)}</div></article>
      <article className="analytics-panel"><p className="eyebrow">Health factors</p><h2>Score composition</h2><div className="factor-list">{data.projectHealth.factors.map(f=><div key={f.name}><div><span>{f.name}</span><strong>{f.value.toFixed(1)}{f.unit==='%'?'%':''}</strong></div><div className="progress"><i style={{width:`${Math.min(100,f.value)}%`}}/></div><small>Weight {f.weight}% · Target {f.target}{f.unit==='%'?'%':''}</small></div>)}</div></article>
      <article className="analytics-panel span-2"><p className="eyebrow">Team capacity</p><h2>Workload distribution</h2><div className="team-table"><div className="team-row head"><span>Engineer</span><span>Assigned</span><span>Completed</span><span>Incidents</span><span>Capacity</span></div>{data.teamLoad.map(x=><div className="team-row" key={x.member}><strong>{x.member}</strong><span>{x.assigned}</span><span>{x.completed}</span><span>{x.incidents}</span><div className="capacity"><i style={{width:`${x.capacityPercent}%`}}/><b>{x.capacityPercent}%</b></div></div>)}</div></article>
      <article className="analytics-panel"><p className="eyebrow">Risk radar</p><h2>Attention required</h2><div className="risk-list">{data.risks.map(r=><div className={`risk ${r.severity.toLowerCase()}`} key={r.title}><span>{r.severity}</span><div><strong>{r.title}</strong><p>{r.description}</p><small>{r.area}</small></div></div>)}</div></article>
    </section>
  </main>
}
