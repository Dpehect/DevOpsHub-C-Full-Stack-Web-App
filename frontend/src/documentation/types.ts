export type WorkspaceSummary = { id:string; name:string; slug:string; description:string; memberCount:number; currentUserRole:string };
export type WikiSpace = { id:string; name:string; slug:string; description:string; documentCount:number };
export type WikiDocumentList = { id:string; title:string; slug:string; category:string; status:string; isFavorite:boolean; updatedAtUtc:string };
export type WikiRevision = { id:string; title:string; category:string; editorId:string; createdAtUtc:string };
export type WikiDocument = WikiDocumentList & { wikiSpaceId:string; content:string; revisions:WikiRevision[] };
