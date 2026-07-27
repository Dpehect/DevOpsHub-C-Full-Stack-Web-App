import { request } from './api/client';
export type SearchItem={id:string;type:string;title:string;subtitle:string;status:string;reference?:string;updatedAt:string;score:number;metadata:Record<string,string>};
export type SearchResponse={query:string;page:number;pageSize:number;total:number;items:SearchItem[];types:{value:string;count:number}[];statuses:{value:string;count:number}[];elapsedMilliseconds:number};
export async function globalSearch(q:string,types:string[],statuses:string[],sort:string,page=1){const p=new URLSearchParams({q,sort,page:String(page),pageSize:'20'});types.forEach(x=>p.append('type',x));statuses.forEach(x=>p.append('status',x));return request<SearchResponse>(`/api/search?${p}`)}
