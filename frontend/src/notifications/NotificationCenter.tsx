import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Bell, CheckCheck, CircleAlert, GitBranch, Info, Rocket, Trash2, X } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { getSession } from '../api/client';
import type { NotificationItem, NotificationPage } from './types';

const apiUrl=import.meta.env.VITE_API_URL??'http://localhost:5080/api';
const hubUrl=apiUrl.replace(/\/api\/?$/,'')+'/hubs/notifications';
const iconMap={Info:Info,Success:Rocket,Warning:CircleAlert,Error:CircleAlert,Assignment:CheckCheck,Mention:Bell,Pipeline:GitBranch,Incident:CircleAlert};
async function call(path:string,options:RequestInit={}){const token=getSession()?.accessToken;const r=await fetch(apiUrl+path,{...options,headers:{'Content-Type':'application/json',Authorization:`Bearer ${token}`,...options.headers}});if(!r.ok)throw new Error('Notification request failed');return r.status===204?undefined:r.json()}

export function NotificationCenter(){
 const [open,setOpen]=useState(false),[data,setData]=useState<NotificationPage>({items:[],unreadCount:0,totalCount:0}),[connected,setConnected]=useState(false);
 const token=getSession()?.accessToken;
 async function load(){setData(await call('/notifications'))}
 useEffect(()=>{load().catch(()=>undefined)},[]);
 useEffect(()=>{if(!token)return;const connection=new HubConnectionBuilder().withUrl(hubUrl,{accessTokenFactory:()=>getSession()?.accessToken??''}).withAutomaticReconnect().configureLogging(LogLevel.Warning).build();connection.on('notificationReceived',(item:NotificationItem)=>setData(x=>({items:[item,...x.items],unreadCount:x.unreadCount+1,totalCount:x.totalCount+1})));connection.onreconnected(()=>setConnected(true));connection.onclose(()=>setConnected(false));connection.start().then(()=>setConnected(true)).catch(()=>setConnected(false));return()=>{connection.stop()}},[token]);
 const unread=useMemo(()=>data.unreadCount,[data]);
 async function read(item:NotificationItem){if(item.isRead)return;await call(`/notifications/${item.id}/read`,{method:'PATCH'});setData(x=>({...x,unreadCount:Math.max(0,x.unreadCount-1),items:x.items.map(n=>n.id===item.id?{...n,isRead:true}:n)}))}
 async function readAll(){await call('/notifications/read-all',{method:'POST'});setData(x=>({...x,unreadCount:0,items:x.items.map(n=>({...n,isRead:true}))}))}
 async function remove(id:string){await call(`/notifications/${id}`,{method:'DELETE'});setData(x=>{const n=x.items.find(i=>i.id===id);return{...x,totalCount:x.totalCount-1,unreadCount:n&&!n.isRead?Math.max(0,x.unreadCount-1):x.unreadCount,items:x.items.filter(i=>i.id!==id)}})}
 return <div className="notification-root"><button className="notification-trigger" onClick={()=>setOpen(!open)} aria-label="Notifications"><Bell size={19}/>{unread>0&&<span>{unread>9?'9+':unread}</span>}</button>{open&&<div className="notification-panel"><div className="notification-head"><div><strong>Notifications</strong><small><i className={connected?'live':''}/>{connected?'Live':'Offline'}</small></div><div><button onClick={readAll} title="Mark all read"><CheckCheck size={17}/></button><button onClick={()=>setOpen(false)}><X size={17}/></button></div></div><div className="notification-list">{data.items.length===0?<div className="notification-empty">You are all caught up.</div>:data.items.map(item=>{const Icon=iconMap[item.type];return <article key={item.id} className={item.isRead?'read':''} onClick={()=>read(item)}><div className={`notification-icon ${item.type.toLowerCase()}`}><Icon size={17}/></div><div><div className="notification-title"><strong>{item.title}</strong>{!item.isRead&&<i/>}</div><p>{item.message}</p><small>{item.source??'DevOpsHub'} · {new Date(item.createdAtUtc).toLocaleString()}</small></div><button onClick={e=>{e.stopPropagation();remove(item.id)}}><Trash2 size={15}/></button></article>})}</div><div className="notification-foot"><button onClick={()=>call('/notifications/demo',{method:'POST'})}>Send live demo notification</button></div></div>}</div>
}
