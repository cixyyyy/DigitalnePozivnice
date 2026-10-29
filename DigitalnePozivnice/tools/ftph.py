import numpy as np, cv2
from PIL import Image
rng=np.random.default_rng(7)
OUT="/home/claude/W/wwwroot/img/fotomat/"
W,H=360,420
def person(img,cx,cy,hs,tilt,tone):
    # glava + ramena kao mekane elipse
    cv2.ellipse(img,(int(cx),int(cy)),(int(hs*.78),int(hs)),tilt,0,360,tone,-1)
    cv2.ellipse(img,(int(cx+tilt*1.2),int(cy+hs*2.35)),(int(hs*1.9),int(hs*1.5)),0,0,360,tone*0.82,-1)
    cv2.rectangle(img,(int(cx-hs*.3),int(cy+hs*.7)),(int(cx+hs*.3),int(cy+hs*1.5)),tone*0.9,-1)
def make(i,spec):
    bg=np.zeros((H,W),np.float32)
    yy,xx=np.mgrid[0:H,0:W]
    bg+=170-70*np.sqrt(((xx-W/2)/(W*.75))**2+((yy-H*.45)/(H*.8))**2)
    fg=np.zeros((H,W),np.float32)
    for (cx,cy,hs,tl,tn) in spec: person(fg,cx,cy,hs,tl,tn)
    m=(fg>0).astype(np.float32); m=cv2.GaussianBlur(m,(0,0),5)
    fg=cv2.GaussianBlur(fg,(0,0),6)
    img=bg*(1-m)+fg*m
    img=cv2.GaussianBlur(img,(0,0),2.2)
    img=np.clip(img,0,255)
    img=(img-img.min())/(img.max()-img.min())*215+20
    img+=rng.normal(0,10,img.shape)
    vig=1-0.35*(((xx-W/2)/(W/2))**2+((yy-H/2)/(H/2))**2)
    img=np.clip(img*vig,0,255).astype(np.uint8)
    Image.fromarray(img).convert("RGB").save(OUT+"ph-%d.webp"%i,quality=80)
make(1,[(135,150,48,-6,95),(225,160,46,8,60)])
make(2,[(150,165,50,10,70),(215,150,47,-10,105)])
make(3,[(180,150,52,0,80)])
make(4,[(125,170,44,-12,60),(235,165,45,12,100)])
print("ok")
