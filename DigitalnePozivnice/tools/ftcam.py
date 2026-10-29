import numpy as np, cv2, json
from PIL import Image, ImageDraw
import sys; sys.path.insert(0,'/home/claude/tools')
from ftcut2 import A,OUT
cam=np.array(Image.open(OUT+"camera.png").convert("RGBA")); h,w=cam.shape[:2]
print(w,h)
rgb=cam[...,:3].copy()
# ukloni natpis (zastitni znak) izmedju ornamenata: pronadji zlatne piksele u traci oko y=530..585 (crop koord.)
off=(212,143)
x0,x1,y0,y1=int(560-off[0]),int(700-off[0]),int(522-off[1]),int(578-off[1])
reg=rgb[y0:y1,x0:x1].astype(np.int32)
bg=np.median(reg.reshape(-1,3),axis=0)
diff=np.abs(reg-bg).sum(axis=2)
m=np.zeros((h,w),np.uint8); m[y0:y1,x0:x1]=(diff>40).astype(np.uint8)*255
m=cv2.dilate(m,np.ones((5,5),np.uint8),iterations=2)
rgb2=cv2.inpaint(rgb,m,6,cv2.INPAINT_TELEA)
cam[...,:3]=rgb2
Image.fromarray(cam,"RGBA").save(OUT+"camera_clean.png")
lum=cv2.cvtColor(rgb2,cv2.COLOR_RGB2GRAY)
# prorez
reg=np.zeros((h,w),bool); reg[int(h*.40):int(h*.58),int(w*.05):int(w*.95)]=True
sl=((lum<60)&reg&(cam[...,3]>200)).astype(np.uint8)
n,lab,st,_=cv2.connectedComponentsWithStats(sl); k=1+np.argmax(st[1:,4]); sx,sy,sw,sh=st[k,:4]
# blic: svijetao pravougaonik gore lijevo
reg2=np.zeros((h,w),bool); reg2[int(h*.04):int(h*.22),int(w*.05):int(w*.35)]=True
fl=((lum>205)&reg2).astype(np.uint8)
n,lab,st,_=cv2.connectedComponentsWithStats(fl); k=1+np.argmax(st[1:,4]); fx,fy,fw,fh=st[k,:4]
print("slot",sx,sy,sw,sh,"flash",fx,fy,fw,fh)
g={"aspect":round(w/h,4),"slot":[round(v,2) for v in (sx/w*100,sy/h*100,sw/w*100,sh/h*100)],"flash":[round(v,2) for v in (fx/w*100,fy/h*100,fw/w*100,fh/h*100)],
   "button":[round(186/w*100,2),round(304/h*100,2),round(100/w*100,2)]}
json.dump(g,open("/tmp/ft_cam.json","w")); print(g)
# provjera: iscrtaj
prev=Image.new("RGB",(w,h),(245,235,221)); prev.paste(Image.fromarray(cam,"RGBA"),(0,0),Image.fromarray(cam,"RGBA"))
d=ImageDraw.Draw(prev)
d.rectangle([sx,sy,sx+sw,sy+sh],outline=(255,0,0),width=3); d.rectangle([fx,fy,fx+fw,fy+fh],outline=(0,0,255),width=3)
bx,by,br=186,304,50; d.ellipse([bx-br,by-br,bx+br,by+br],outline=(0,160,0),width=3)
prev.save(OUT+"cam_check.png")
