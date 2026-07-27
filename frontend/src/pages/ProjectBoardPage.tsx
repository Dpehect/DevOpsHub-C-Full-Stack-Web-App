import { useMemo, useState } from 'react'
import { AlertCircle, BarChart3, CalendarDays, CheckCircle2, ChevronDown, Filter, GripVertical, Plus, Search, SlidersHorizontal, Users, X } from 'lucide-react'
import { api } from '../api/client'
import type { Priority, ProjectBoard, WorkItem, WorkItemType, WorkStatus } from '../projects/types'

const seed:ProjectBoard={project:{id:'demo',name:'DevOpsHub Platform',key:'DOH',description:'Engineering operations workspace',openItems:12,completedItems:18},sprints:[{id:'s1',name:'Sprint 04',goal:'Ship project management core',status:'Active',startDate:'2026-07-21',endDate:'2026-08-03'}],epics:[{id:'e1',title:'Platform foundation',color:'#8b5cf6',itemCount:7},{id:'e2',title:'Developer experience',color:'#06b6d4',itemCount:5}],items:[
{id:'1',key:'DOH-41',title:'Project board API',description:'Expose project board data with ordering and status transitions.',type:'Story',status:'Done',priority:'High',storyPoints:5,assigneeName:'Alex Morgan',position:0,sprintId:'s1',epicId:'e1'},
{id:'2',key:'DOH-42',title:'Persist column ordering',description:'Save drag-and-drop ordering after every board move.',type:'Task',status:'InReview',priority:'Medium',storyPoints:3,assigneeName:'Maya Chen',position:0,sprintId:'s1',epicId:'e1'},
{id:'3',key:'DOH-43',title:'Permission boundary tests',type:'Task',status:'InProgress',priority:'High',storyPoints:5,assigneeName:'Jordan Lee',position:0,sprintId:'s1',epicId:'e1'},
{id:'4',key:'DOH-44',title:'Fix stale sprint totals',type:'Bug',status:'Todo',priority:'Critical',storyPoints:3,assigneeName:'Sam Rivera',position:0,sprintId:'s1',epicId:'e2',dueDate:'2026-07-30'},
{id:'5',key:'DOH-45',title:'Epic progress cards',type:'Story',status:'Backlog',priority:'Low',storyPoints:5,position:0,epicId:'e2'},
{id:'6',key:'DOH-46',title:'Keyboard board navigation',type:'Task',status:'Backlog',priority:'Medium',storyPoints:2,position:1,epicId:'e2'},
{id:'7',key:'DOH-47',title:'Add WIP limit indicators',type:'Task',status:'Todo',priority:'Medium',storyPoints:2,assigneeName:'Maya Chen',position:1,sprintId:'s1'},
{id:'8',key:'DOH-48',title:'Board activity timeline',type:'Story',status:'InProgress',priority:'High',storyPoints:5,assigneeName:'Alex Morgan',position:1,sprintId:'s1'},
]}

const columns:WorkStatus[]=['Backlog','Todo','InProgress','InReview','Done']
const label:Record<WorkStatus,string>={Backlog:'Backlog',Todo:'To do',InProgress:'In progress',InReview:'In review',Done:'Done'}
const wip:Partial<Record<WorkStatus,number>>={Todo:5,InProgress:4,InReview:3}

export default function ProjectBoardPage(){
 const [board,setBoard]=useState(seed)
 const [query,setQuery]=useState('')
 const [priority,setPriority]=useState<'All'|Priority>('All')
 const [type,setType]=useState<'All'|WorkItemType>('All')
 const [assignee,setAssignee]=useState<'All'|'Assigned'|'Unassigned'>('All')
 const [dragged,setDragged]=useState<string|null>(null)
 const [dragOver,setDragOver]=useState<{status:WorkStatus;index:number}|null>(null)
 const [selected,setSelected]=useState<WorkItem|null>(null)
 const [creating,setCreating]=useState<WorkStatus|null>(null)
 const [toast,setToast]=useState('')

 const filtered=useMemo(()=>board.items.filter(x=>{
  const text=(x.title+' '+x.key+' '+(x.assigneeName??'')).toLowerCase()
  return text.includes(query.toLowerCase())&&(priority==='All'||x.priority===priority)&&(type==='All'||x.type===type)&&(assignee==='All'||(assignee==='Assigned'?!!x.assigneeName:!x.assigneeName))
 }),[board.items,query,priority,type,assignee])
 const activeSprint=board.sprints.find(x=>x.status==='Active')??board.sprints[0]
 const sprintItems=board.items.filter(x=>x.sprintId===activeSprint?.id)
 const points=sprintItems.reduce((s,x)=>s+x.storyPoints,0)
 const donePoints=sprintItems.filter(x=>x.status==='Done').reduce((s,x)=>s+x.storyPoints,0)
 const completion=points?Math.round(donePoints/points*100):0

 function ordered(status:WorkStatus){return filtered.filter(x=>x.status===status).sort((a,b)=>a.position-b.position)}
 function showToast(message:string){setToast(message);window.setTimeout(()=>setToast(''),2200)}
 async function move(status:WorkStatus,index:number){
  if(!dragged)return
  const previous=board.items
  const moving=previous.find(x=>x.id===dragged)
  if(!moving)return
  const destination=previous.filter(x=>x.status===status&&x.id!==dragged).sort((a,b)=>a.position-b.position)
  destination.splice(index,0,{...moving,status})
  const updated=previous.map(x=>x.id===dragged?{...x,status,position:index}:x).map(x=>{
   if(x.id===dragged||x.status!==status)return x
   const i=destination.findIndex(d=>d.id===x.id)
   return i<0?x:{...x,position:i}
  })
  setBoard(b=>({...b,items:updated}));setDragged(null);setDragOver(null)
  if(moving.id!=='demo'&&!/^\d+$/.test(moving.id)){
   try{await api.moveItem(moving.id,status,index,moving.sprintId)}catch{setBoard(b=>({...b,items:previous}));showToast('Move could not be saved')}
  } else showToast(`${moving.key} moved to ${label[status]}`)
 }
 function createItem(input:{title:string;type:WorkItemType;priority:Priority;storyPoints:number}){
  const status=creating??'Backlog'
  const next=board.items.length+41
  const item:WorkItem={id:crypto.randomUUID(),key:`${board.project.key}-${next}`,title:input.title,type:input.type,status,priority:input.priority,storyPoints:input.storyPoints,position:ordered(status).length,sprintId:status==='Backlog'?undefined:activeSprint?.id}
  setBoard(b=>({...b,items:[...b.items,item]}));setCreating(null);showToast(`${item.key} created`)
 }
 return <main className="board-shell phase-five">
  {toast&&<div className="board-toast"><CheckCircle2 size={16}/>{toast}</div>}
  <header className="board-header"><div><div className="breadcrumb">Projects <span>/</span> {board.project.key} <span>/</span> Board</div><h1>{board.project.name}</h1><p>{board.project.description}</p></div><div className="board-header-actions"><button className="board-secondary"><BarChart3 size={16}/>Insights</button><button className="primary" onClick={()=>setCreating('Backlog')}><Plus size={16}/>Create work item</button></div></header>
  <section className="sprint-strip"><div className="sprint-title"><span className="status-dot"/><div><small>ACTIVE SPRINT</small><strong>{activeSprint?.name}</strong></div></div><div className="sprint-goal"><small>SPRINT GOAL</small><span>{activeSprint?.goal}</span></div><div className="metric"><b>{points}</b><small>Total points</small></div><div className="metric"><b>{donePoints}</b><small>Done points</small></div><div className="metric progress-metric"><div><b>{completion}%</b><small>Completed</small></div><div className="progress"><span style={{width:`${completion}%`}}/></div></div></section>
  <div className="board-tools"><label className="board-search"><Search size={15}/><input placeholder="Search by key, title or assignee…" value={query} onChange={e=>setQuery(e.target.value)}/>{query&&<button onClick={()=>setQuery('')}><X size={14}/></button>}</label><FilterSelect icon={<AlertCircle size={14}/>} value={priority} onChange={v=>setPriority(v as 'All'|Priority)} options={['All','Low','Medium','High','Critical']}/><FilterSelect icon={<SlidersHorizontal size={14}/>} value={type} onChange={v=>setType(v as 'All'|WorkItemType)} options={['All','Task','Story','Bug','SubTask']}/><FilterSelect icon={<Users size={14}/>} value={assignee} onChange={v=>setAssignee(v as 'All'|'Assigned'|'Unassigned')} options={['All','Assigned','Unassigned']}/><span className="item-count">{filtered.length} items</span></div>
  <section className="kanban">{columns.map(status=>{const items=ordered(status);const limit=wip[status];const over=limit?items.length>=limit:false;return <div className={`kanban-column ${dragOver?.status===status?'is-over':''}`} key={status} onDragOver={e=>{e.preventDefault();if(!dragOver||dragOver.status!==status)setDragOver({status,index:items.length})}} onDrop={()=>move(status,dragOver?.status===status?dragOver.index:items.length)}><div className="column-head"><div><span className={`column-dot ${status.toLowerCase()}`}/><b>{label[status]}</b><em>{items.length}</em>{limit&&<small className={over?'wip-over':''}>WIP {items.length}/{limit}</small>}</div><button onClick={()=>setCreating(status)} aria-label={`Add to ${label[status]}`}><Plus size={17}/></button></div><div className="cards">{items.map((item,index)=><div key={item.id} onDragOver={e=>{e.preventDefault();e.stopPropagation();setDragOver({status,index})}}><Card item={item} onDrag={()=>setDragged(item.id)} onOpen={()=>setSelected(item)}/>{dragged&&dragOver?.status===status&&dragOver.index===index&&<div className="drop-indicator"/>}</div>)}{items.length===0&&<button className="empty-column" onClick={()=>setCreating(status)}><Plus size={16}/>Add work item</button>}</div></div>})}</section>
  {selected&&<DetailDrawer item={selected} epic={board.epics.find(x=>x.id===selected.epicId)} onClose={()=>setSelected(null)}/>} 
  {creating&&<CreateModal status={creating} onClose={()=>setCreating(null)} onCreate={createItem}/>} 
 </main>
}

function FilterSelect({icon,value,onChange,options}:{icon:React.ReactNode;value:string;onChange:(value:string)=>void;options:string[]}){return <label className="filter-select">{icon}<select value={value} onChange={e=>onChange(e.target.value)}>{options.map(x=><option key={x}>{x}</option>)}</select><ChevronDown size={13}/></label>}

function Card({item,onDrag,onOpen}:{item:WorkItem;onDrag:()=>void;onOpen:()=>void}){return <article className="work-card" draggable onDragStart={e=>{e.dataTransfer.effectAllowed='move';onDrag()}} onClick={onOpen}><div className="card-top"><div className="drag-key"><GripVertical size={14}/><code>{item.key}</code></div><span className={`priority ${item.priority.toLowerCase()}`}>{item.priority}</span></div><h3>{item.title}</h3><div className="card-tags"><span className={`type ${item.type.toLowerCase()}`}>{item.type}</span>{item.dueDate&&<span className="due"><CalendarDays size={11}/>{item.dueDate.slice(5)}</span>}</div><footer><div className="assignee"><div className="avatar">{item.assigneeName?.split(' ').map(x=>x[0]).join('')||'—'}</div><span>{item.assigneeName||'Unassigned'}</span></div><span className="points">{item.storyPoints||'–'} pts</span></footer></article>}

function DetailDrawer({item,epic,onClose}:{item:WorkItem;epic?:{title:string;color:string};onClose:()=>void}){return <div className="drawer-backdrop" onMouseDown={onClose}><aside className="detail-drawer" onMouseDown={e=>e.stopPropagation()}><header><div><code>{item.key}</code><span className={`type ${item.type.toLowerCase()}`}>{item.type}</span></div><button onClick={onClose}><X size={18}/></button></header><h2>{item.title}</h2><p>{item.description||'No description has been added to this work item.'}</p><div className="drawer-grid"><Detail label="Status" value={label[item.status]}/><Detail label="Priority" value={item.priority}/><Detail label="Story points" value={String(item.storyPoints||'Not estimated')}/><Detail label="Assignee" value={item.assigneeName||'Unassigned'}/><Detail label="Epic" value={epic?.title||'No epic'}/><Detail label="Due date" value={item.dueDate||'No due date'}/></div><section className="activity"><h3>Activity</h3><div><span className="activity-dot"/><p><strong>Board updated</strong><small>Work item details are available in the current project scope.</small></p></div></section></aside></div>}
function Detail({label,value}:{label:string;value:string}){return <div><small>{label}</small><strong>{value}</strong></div>}

function CreateModal({status,onClose,onCreate}:{status:WorkStatus;onClose:()=>void;onCreate:(input:{title:string;type:WorkItemType;priority:Priority;storyPoints:number})=>void}){
 const [title,setTitle]=useState('');const [type,setType]=useState<WorkItemType>('Task');const [priority,setPriority]=useState<Priority>('Medium');const [storyPoints,setStoryPoints]=useState(3)
 return <div className="modal-backdrop" onMouseDown={onClose}><form className="modal board-modal" onMouseDown={e=>e.stopPropagation()} onSubmit={e=>{e.preventDefault();if(title.trim())onCreate({title:title.trim(),type,priority,storyPoints})}}><div className="modal-title"><div><small>NEW WORK ITEM</small><h2>Add to {label[status]}</h2></div><button type="button" onClick={onClose}><X size={18}/></button></div><label>Title<input autoFocus value={title} onChange={e=>setTitle(e.target.value)} placeholder="Describe the work clearly" required/></label><div className="form-grid"><label>Type<select value={type} onChange={e=>setType(e.target.value as WorkItemType)}><option>Task</option><option>Story</option><option>Bug</option><option>SubTask</option></select></label><label>Priority<select value={priority} onChange={e=>setPriority(e.target.value as Priority)}><option>Low</option><option>Medium</option><option>High</option><option>Critical</option></select></label></div><label>Story points<input type="number" min="0" max="100" value={storyPoints} onChange={e=>setStoryPoints(Number(e.target.value))}/></label><div className="modal-actions"><button type="button" className="board-secondary" onClick={onClose}>Cancel</button><button className="primary" disabled={!title.trim()}>Create item</button></div></form></div>
}
