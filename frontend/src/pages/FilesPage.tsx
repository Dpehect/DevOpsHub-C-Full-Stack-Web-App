import { useMemo, useState } from 'react';
import { ArrowLeft, FileText, Folder, History, Search, UploadCloud } from 'lucide-react';

type Asset={id:number;name:string;kind:'folder'|'file';size:string;owner:string;version:number;updated:string;};
const seed:Asset[]=[
{id:1,name:'Architecture',kind:'folder',size:'—',owner:'Platform Team',version:1,updated:'2 min ago'},
{id:2,name:'release-checklist.md',kind:'file',size:'18 KB',owner:'Yunus Emre',version:7,updated:'11 min ago'},
{id:3,name:'incident-postmortem.pdf',kind:'file',size:'1.8 MB',owner:'SRE Team',version:3,updated:'Yesterday'},
{id:4,name:'api-contracts.json',kind:'file',size:'84 KB',owner:'Backend Team',version:12,updated:'Yesterday'},
{id:5,name:'design-system.fig',kind:'file',size:'7.2 MB',owner:'Product Design',version:4,updated:'3 days ago'}];
export default function FilesPage({onBack}:{onBack:()=>void}){
 const [q,setQ]=useState(''); const [selected,setSelected]=useState<Asset|null>(seed[1]);
 const rows=useMemo(()=>seed.filter(x=>x.name.toLowerCase().includes(q.toLowerCase())),[q]);
 return <main className="app-shell"><header className="page-head"><button className="icon-btn" onClick={onBack}><ArrowLeft/></button><div><p className="eyebrow">PHASE 19</p><h1>Files & version control</h1><p>Workspace assets, immutable revisions and ownership metadata.</p></div><button className="primary"><UploadCloud size={18}/> Upload asset</button></header>
 <section className="kpi-grid"><article className="kpi"><span>Total assets</span><strong>1,284</strong><small>+42 this month</small></article><article className="kpi"><span>Storage</span><strong>18.6 GB</strong><small>of 50 GB local quota</small></article><article className="kpi"><span>Revisions</span><strong>8,904</strong><small>fully auditable</small></article><article className="kpi"><span>Shared links</span><strong>37</strong><small>4 expire today</small></article></section>
 <section className="split-panel"><div className="panel"><div className="toolbar"><div className="searchbox"><Search size={17}/><input value={q} onChange={e=>setQ(e.target.value)} placeholder="Search files"/></div></div><div className="data-table">{rows.map(x=><button key={x.id} className={'table-row '+(selected?.id===x.id?'active':'')} onClick={()=>setSelected(x)}><span className="file-name">{x.kind==='folder'?<Folder/>:<FileText/>}<b>{x.name}</b></span><span>{x.owner}</span><span>v{x.version}</span><span>{x.size}</span><span>{x.updated}</span></button>)}</div></div>
 <aside className="panel detail-card">{selected&&<><div className="detail-icon">{selected.kind==='folder'?<Folder/>:<FileText/>}</div><p className="eyebrow">SELECTED ASSET</p><h2>{selected.name}</h2><dl><div><dt>Owner</dt><dd>{selected.owner}</dd></div><div><dt>Current version</dt><dd>v{selected.version}</dd></div><div><dt>Updated</dt><dd>{selected.updated}</dd></div></dl><h3><History size={17}/> Revision history</h3>{[0,1,2].map(i=><div className="history-item" key={i}><b>v{Math.max(1,selected.version-i)}</b><span>{i===0?'Current revision':'Previous revision'}</span><small>{i+1} day ago</small></div>)}</>}</aside></section></main>;
}