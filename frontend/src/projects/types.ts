export type WorkStatus='Backlog'|'Todo'|'InProgress'|'InReview'|'Done'
export type Priority='Low'|'Medium'|'High'|'Critical'
export type WorkItemType='Task'|'Story'|'Bug'|'SubTask'
export interface WorkItem { id:string; key:string; title:string; description?:string; type:WorkItemType; status:WorkStatus; priority:Priority; storyPoints:number; assigneeId?:string; assigneeName?:string; sprintId?:string; epicId?:string; dueDate?:string; position:number }
export interface ProjectBoard { project:{id:string;name:string;key:string;description?:string;openItems:number;completedItems:number}; items:WorkItem[]; sprints:{id:string;name:string;goal?:string;status:string;startDate?:string;endDate?:string}[]; epics:{id:string;title:string;color:string;itemCount:number}[] }
