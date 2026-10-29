import numpy as np, cv2
from PIL import Image
import sys; sys.path.insert(0,'/home/claude/tools')
from ftcut2 import A,OUT
def glowcut(path,region):
    im=np.array(Image.open(path).convert("RGB")); x0,y0,x1,y1=region; im=im[y0:y1,x0:x1].copy()
    g=cv2.cvtColor(im,cv2.COLOR_RGB2GRAY).astype(np.float32)
    hp=np.abs(g-cv2.GaussianBlur(g,(0,0),3))
    tex=cv2.GaussianBlur(hp,(0,0),2.5)
    m=(tex>2.2).astype(np.uint8)
    m=cv2.morphologyEx(m,cv2.MORPH_CLOSE,cv2.getStructuringElement(cv2.MORPH_ELLIPSE,(25,25)))
    # popuni rupe
    cnts,_=cv2.findContours(m,cv2.RETR_EXTERNAL,cv2.CHAIN_APPROX_SIMPLE)
    filled=np.zeros_like(m); 
    for c in cnts:
        if cv2.contourArea(c)>3000: cv2.drawContours(filled,[c],-1,1,-1)
    # grabcut doradi ivice
    gc=np.where(filled>0,cv2.GC_PR_FGD,cv2.GC_PR_BGD).astype(np.uint8)
    er=cv2.erode(filled,np.ones((15,15),np.uint8)); gc[er>0]=cv2.GC_FGD
    di=cv2.dilate(filled,np.ones((41,41),np.uint8)); gc[di==0]=cv2.GC_BGD
    bgd=np.zeros((1,65),np.float64); fgd=np.zeros((1,65),np.float64)
    cv2.grabCut(im,gc,None,bgd,fgd,4,cv2.GC_INIT_WITH_MASK)
    fg=((gc==cv2.GC_FGD)|(gc==cv2.GC_PR_FGD)).astype(np.uint8)
    # unutrasnje rupe (sjaj izmedju svitaka): velike glatke povrsine unutar maske
    smooth=((tex<1.6)&(fg>0)).astype(np.uint8)
    smooth=cv2.morphologyEx(smooth,cv2.MORPH_OPEN,np.ones((5,5),np.uint8))
    n,lab,st,_=cv2.connectedComponentsWithStats(smooth)
    for k in range(1,n):
        if st[k,cv2.CC_STAT_AREA]>2200: fg[lab==k]=0
    fg=cv2.morphologyEx(fg,cv2.MORPH_OPEN,np.ones((3,3),np.uint8))
    a=cv2.GaussianBlur(fg.astype(np.float32),(0,0),1.1); a=np.clip((a-0.4)/0.45,0,1)
    rgba=np.dstack([im,(a*255).astype(np.uint8)])
    ys,xs=np.where(a>0.02); return rgba[ys.min():ys.max()+1,xs.min():xs.max()+1]
if __name__=="__main__":
    tiles=[]
    for n,fn,reg in [("o3","a06140d0-image.png",(0,0,768,1024)),("o4","a06140d0-image.png",(768,0,1536,1024)),("o5","aa2c33a6-image.png",(0,0,768,1024)),("o6","aa2c33a6-image.png",(768,0,1536,1024))]:
        r=glowcut(A+fn,reg); Image.fromarray(r,"RGBA").save(OUT+n+".png"); tiles.append(Image.fromarray(r,"RGBA"))
    sheet=Image.new("RGB",(1400,700),(245,235,221)); x=10
    for t in tiles:
        t2=t.copy(); t2.thumbnail((320,680)); sheet.paste(t2,(x,10),t2); x+=340
    sheet.save(OUT+"orn_preview2.png")
