import json, numpy as np, cv2
from PIL import Image, ImageFilter
import ftcut
OUT="/home/claude/W/wwwroot/img/fotomat/"
S=ftcut.OUT
for i in range(8): ftcut.cut(i)
geo={}
def load(n): return np.array(Image.open(S+n+".png").convert("RGBA"))
def up(a,f=2.0):
    im=Image.fromarray(a,"RGBA")
    # alfa kanal se mora uvecavati odvojeno (premultiplied), da ivice ne dobiju crni rub
    r=np.array(im).astype(np.float32); al=r[...,3:4]/255.0
    pm=Image.fromarray((r[...,:3]*al).astype(np.uint8)); aa=Image.fromarray(r[...,3].astype(np.uint8))
    sz=(int(a.shape[1]*f),int(a.shape[0]*f))
    pm=pm.resize(sz,Image.LANCZOS).filter(ImageFilter.UnsharpMask(1.2,70,2)); aa=aa.resize(sz,Image.LANCZOS)
    pa=np.array(pm).astype(np.float32); al2=np.array(aa).astype(np.float32)[...,None]/255.0
    rgb=np.where(al2>0.01,pa/np.maximum(al2,0.01),0).clip(0,255).astype(np.uint8)
    return np.dstack([rgb,np.array(aa)])
def save(a,name,q=88):
    Image.fromarray(a,"RGBA").save(OUT+name,quality=q,method=6)
def trim(a):
    ys,xs=np.where(a[...,3]>5)
    return a[ys.min():ys.max()+1,xs.min():xs.max()+1]
def rotate(a,ang):
    h,w=a.shape[:2]
    M=cv2.getRotationMatrix2D((w/2,h/2),ang,1.0)
    cos,sin=abs(M[0,0]),abs(M[0,1])
    nw,nh=int(h*sin+w*cos),int(h*cos+w*sin)
    M[0,2]+=nw/2-w/2; M[1,2]+=nh/2-h/2
    return cv2.warpAffine(a,M,(nw,nh),flags=cv2.INTER_CUBIC,borderValue=(0,0,0,0))
def pct(r,w,h): return [round(r[0]/w*100,2),round(r[1]/h*100,2),round(r[2]/w*100,2),round(r[3]/h*100,2)]  # x,y,w,h u %

# --- sanduce
mb=load("mailbox"); h,w=mb.shape[:2]
lum=cv2.cvtColor(mb[...,:3],cv2.COLOR_RGB2GRAY)
reg=np.zeros_like(lum,bool); reg[int(h*.22):int(h*.40),int(w*.12):int(w*.95)]=True
slot=((lum<34)&reg&(mb[...,3]>200)).astype(np.uint8)
n,lab,st,_=cv2.connectedComponentsWithStats(slot)
k=1+np.argmax(st[1:,cv2.CC_STAT_AREA]); x,y,ww,hh=st[k,:4]
geo["mailbox"]={"slot":pct((x,y,ww,hh),w,h),"aspect":round(w/h,4)}
save(up(mb,2),"mailbox.webp")

# --- koverta, srce, cvijet, lanac
for n in ("envelope","heart","flower","chain"):
    a=load(n); geo[n]={"aspect":round(a.shape[1]/a.shape[0],4)}; save(up(a,1.6),n+".webp")

# --- kartica: ukloni monogram (inpaint) da tekst ide uzivo
cd=load("card"); h,w=cd.shape[:2]
rgb=cd[...,:3].copy()
bgcol=np.median(rgb[int(h*.48):int(h*.52),int(w*.42):int(w*.58)].reshape(-1,3),axis=0)
reg=np.zeros((h,w),np.uint8); reg[int(h*.30):int(h*.66),int(w*.26):int(w*.74)]=255
diff=np.abs(rgb.astype(np.int32)-bgcol[None,None,:]).sum(axis=2)
m=((diff>34)&(reg>0)).astype(np.uint8)*255
m=cv2.dilate(m,np.ones((5,5),np.uint8),iterations=2)
inp=cv2.inpaint(rgb,m,5,cv2.INPAINT_TELEA)
cd[...,:3]=inp
geo["card"]={"aspect":round(w/h,4)}
save(up(cd,1.6),"card.webp")

# --- traka: perspektivno ispravi (uspravna pravougaona traka), izrezi 4 prozora
def quad_of(a):
    cnts,_=cv2.findContours((a[...,3]>128).astype(np.uint8),cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
    c=max(cnts,key=cv2.contourArea)
    hull=cv2.convexHull(c)
    for eps in np.linspace(0.01,0.08,40):
        ap=cv2.approxPolyDP(hull,eps*cv2.arcLength(hull,True),True)
        if len(ap)==4: return ap.reshape(4,2).astype(np.float32)
    raise SystemExit("no quad")
def order(q):
    s=q.sum(1); d=np.diff(q,axis=1).ravel()
    return np.array([q[np.argmin(s)],q[np.argmin(d)],q[np.argmax(s)],q[np.argmax(d)]],np.float32)  # tl,tr,br,bl
def rectify(a,q,outw=None):
    q=order(q)
    wt=np.linalg.norm(q[1]-q[0]); wb=np.linalg.norm(q[2]-q[3]); hl=np.linalg.norm(q[3]-q[0]); hr=np.linalg.norm(q[2]-q[1])
    W=int(round((wt+wb)/2)); H=int(round((hl+hr)/2))
    dst=np.array([[0,0],[W-1,0],[W-1,H-1],[0,H-1]],np.float32)
    M=cv2.getPerspectiveTransform(q,dst)
    return cv2.warpPerspective(a,M,(W,H),flags=cv2.INTER_CUBIC,borderValue=(0,0,0,0))
st=load("strip"); q=quad_of(st); print("strip quad",q.tolist())
r=rectify(st,q); h,w=r.shape[:2]; print("rect",w,h)
lm=cv2.cvtColor(r[...,:3],cv2.COLOR_RGB2GRAY).astype(np.float32)
dark=((lm<150)).astype(np.uint8)
dark[:, :int(w*.08)]=0; dark[:, int(w*.92):]=0
dark=cv2.morphologyEx(dark,cv2.MORPH_OPEN,np.ones((5,5),np.uint8))
n,lab,stt,_=cv2.connectedComponentsWithStats(dark)
boxes=[tuple(int(v) for v in stt[k,:4]) for k in range(1,n) if stt[k,cv2.CC_STAT_AREA]>w*h*0.03]
boxes.sort(key=lambda b:b[1]); print("windows",boxes)
Image.fromarray(r,"RGBA").save(S+"strip_rect.png")

# traka: rupe i izlaz
hole=r.copy()
wins=[]
for (x,y,bw,bh) in boxes:
    hole[y+1:y+bh-1,x+1:x+bw-1,3]=0
    wins.append(pct((x,y,bw,bh),w,h))
geo["strip"]={"aspect":round(w/h,4),"wins":wins}
save(up(hole,2.4),"strip.webp")
for i,(x,y,bw,bh) in enumerate(boxes):
    ph=Image.fromarray(r[y+2:y+bh-2,x+2:x+bw-2,:3])
    ph=ph.resize((ph.width*3,ph.height*3),Image.LANCZOS).filter(ImageFilter.UnsharpMask(1.5,60,2))
    ph.save(OUT+"demo-%d.webp"%(i+1),quality=86)

# --- polaroidi
pl=load("polaroid"); h,w=pl.shape[:2]
rgb=pl[...,:3]; hsv=cv2.cvtColor(rgb,cv2.COLOR_RGB2HSV)
lum=cv2.cvtColor(rgb,cv2.COLOR_RGB2GRAY)
photo=(((lum<190)|(hsv[...,1]>45))&(pl[...,3]>200)).astype(np.uint8)
photo=cv2.morphologyEx(photo,cv2.MORPH_OPEN,np.ones((7,7),np.uint8))
n,lab,stt,_=cv2.connectedComponentsWithStats(photo)
k=1+np.argmax(stt[1:,cv2.CC_STAT_AREA])
cm=(lab==k).astype(np.uint8)
cnts,_=cv2.findContours(cm,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
c=max(cnts,key=cv2.contourArea); (cx,cy),(rw,rh),ang=cv2.minAreaRect(c)
print("polaroid photo rect",cx,cy,rw,rh,ang)
if ang>45: ang-=90
if ang<-45: ang+=90
pr=rotate(pl,ang)  # pozitivan ugao = CCW
# ponovo pronadji prozor u rotiranoj slici
rgb=pr[...,:3]; hsv=cv2.cvtColor(rgb,cv2.COLOR_RGB2HSV); lum=cv2.cvtColor(rgb,cv2.COLOR_RGB2GRAY)
photo=(((lum<190)|(hsv[...,1]>45))&(pr[...,3]>200)).astype(np.uint8)
photo=cv2.morphologyEx(photo,cv2.MORPH_OPEN,np.ones((7,7),np.uint8))
n,lab,stt,_=cv2.connectedComponentsWithStats(photo)
k=1+np.argmax(stt[1:,cv2.CC_STAT_AREA]); x,y,bw,bh=[int(v) for v in stt[k,:4]]
print("rot polaroid window",x,y,bw,bh)
ys,xs=np.where(pr[...,3]>5); y0,y1,x0,x1=ys.min(),ys.max()+1,xs.min(),xs.max()+1
pr=pr[y0:y1,x0:x1]; x-=x0; y-=y0; h,w=pr.shape[:2]
Image.fromarray(pr[y:y+bh,x:x+bw,:3]).resize((bw*3,bh*3),Image.LANCZOS).save(OUT+"demo-grad.webp",quality=86)
hole=pr.copy(); hole[y+2:y+bh-2,x+2:x+bw-2,3]=0
geo["polaroid"]={"aspect":round(w/h,4),"win":pct((x,y,bw,bh),w,h)}
save(up(hole,1.8),"polaroid.webp")
json.dump(geo,open("/tmp/ft_geo.json","w"),indent=1)
print(json.dumps(geo,indent=1))
