import numpy as np, cv2
from PIL import Image
A="/mnt/user-data/attachments/8cf8e492-a911-56d3-bda5-bd8d5647301e/"
OUT="/tmp/claude-0/-home-claude/8cf8e492-a911-56d3-bda5-bd8d5647301e/scratchpad/ft/"
def cutimg(path,thr,chroma=255,erode=1,blur=0.9,region=None,allpix=False,seeds=None):
    im=np.array(Image.open(path).convert("RGB"))
    if region is not None:
        x0,y0,x1,y1=region; im=im[y0:y1,x0:x1].copy()
    h,w=im.shape[:2]
    lum=cv2.cvtColor(im,cv2.COLOR_RGB2GRAY).astype(np.int32)
    ch=im.max(axis=2).astype(np.int32)-im.min(axis=2).astype(np.int32)
    dark=((lum<thr)&(ch<chroma)).astype(np.uint8)
    work=(dark*255).astype(np.uint8); mask=np.zeros((h+2,w+2),np.uint8)
    pts=seeds or [(0,0),(w-1,0),(0,h-1),(w-1,h-1),(w//2,0),(w//2,h-1),(0,h//2),(w-1,h//2)]
    for (sx,sy) in pts:
        if work[sy,sx]==255: cv2.floodFill(work,mask,(sx,sy),128)
    bg=(work==128).astype(np.uint8)
    fg=1-bg
    if allpix: fg=(1-dark).astype(np.uint8)
    # ukloni sitne ostrvce pozadine/prednjeg plana
    n,lab,st,_=cv2.connectedComponentsWithStats(fg)
    if n>1:
        keep=np.zeros_like(fg)
        for k in range(1,n):
            if st[k,cv2.CC_STAT_AREA]>w*h*0.004: keep[lab==k]=1
        fg=keep
    if erode: fg=cv2.erode(fg,np.ones((3,3),np.uint8),iterations=erode)
    a=cv2.GaussianBlur(fg.astype(np.float32),(0,0),blur); a=np.clip((a-0.35)/0.5,0,1)
    rgba=np.dstack([im,(a*255).astype(np.uint8)])
    ys,xs=np.where(a>0.02)
    return rgba[ys.min():ys.max()+1,xs.min():xs.max()+1], (xs.min(),ys.min())
if __name__=="__main__":
    f=lambda i:A+i
    tests=[("o1","f97b7db0-image.png",(0,0,768,1024),40),("o2","f97b7db0-image.png",(768,0,1536,1024),40),
           ("o3","a06140d0-image.png",(0,0,768,1024),95),("o4","a06140d0-image.png",(768,0,1536,1024),95),
           ("o5","aa2c33a6-image.png",(0,0,768,1024),95),("o6","aa2c33a6-image.png",(768,0,1536,1024),95)]
    tiles=[]
    for n,fn,reg,thr in tests:
        r,_=cutimg(f(fn),thr,region=reg,erode=1)
        Image.fromarray(r,"RGBA").save(OUT+n+".png"); tiles.append(Image.fromarray(r,"RGBA")); print(n,r.shape)
    sheet=Image.new("RGB",(2000,700),(245,235,221)); x=10
    for t in tiles:
        t2=t.copy(); t2.thumbnail((320,680)); sheet.paste(t2,(x,10),t2); x+=330
    sheet.save(OUT+"orn_preview.png")
    cam,off=cutimg(f("849377b5-image.png"),30,chroma=22)
    Image.fromarray(cam,"RGBA").save(OUT+"camera.png"); print("camera",cam.shape,off)
