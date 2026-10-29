import numpy as np, cv2
from PIL import Image
SRC="/mnt/user-data/attachments/8cf8e492-a911-56d3-bda5-bd8d5647301e/dd26328a-image.png"
OUT="/tmp/claude-0/-home-claude/8cf8e492-a911-56d3-bda5-bd8d5647301e/scratchpad/ft/"
im=np.array(Image.open(SRC).convert("RGB"))
H,W=im.shape[:2]
names=["mailbox","envelope","card","chain","heart","flower","polaroid","strip"]
CH=16
def cut(idx,thr=46,erode=1,blur=0.9):
    r,c=divmod(idx,4)
    x0,x1=c*W//4,(c+1)*W//4; y0,y1=r*H//2,(r+1)*H//2
    crop=im[y0:y1,x0:x1].copy()
    lum=cv2.cvtColor(crop,cv2.COLOR_RGB2GRAY).astype(np.int32)
    chroma=crop.max(axis=2).astype(np.int32)-crop.min(axis=2).astype(np.int32)
    dark=((lum<thr)&(chroma<CH)).astype(np.uint8)
    # poplavi od ivica samo kroz tamne piksele
    h,w=dark.shape
    flood=np.zeros((h+2,w+2),np.uint8)
    bg=np.zeros((h,w),np.uint8)
    seed=dark.copy()
    mask=np.zeros((h+2,w+2),np.uint8)
    work=(dark*255).astype(np.uint8)
    for (sx,sy) in [(0,0),(w-1,0),(0,h-1),(w-1,h-1),(w//2,0),(w//2,h-1),(0,h//2),(w-1,h//2)]:
        if work[sy,sx]==255:
            cv2.floodFill(work,mask,(sx,sy),128)
    bg=(work==128).astype(np.uint8)
    fg=1-bg
    if names[idx]=='chain': fg=(1-dark).astype(np.uint8)
    if erode: fg=cv2.erode(fg,np.ones((3,3),np.uint8),iterations=erode)
    a=cv2.GaussianBlur(fg.astype(np.float32),(0,0),blur)
    a=np.clip((a-0.35)/0.5,0,1)
    rgba=np.dstack([crop,(a*255).astype(np.uint8)])
    ys,xs=np.where(a>0.02)
    rgba=rgba[ys.min():ys.max()+1,xs.min():xs.max()+1]
    Image.fromarray(rgba,"RGBA").save(OUT+names[idx]+".png")
    return rgba.shape
if __name__=="__main__":
    for i in range(8): print(names[i],cut(i))
    # pregled na kremu
    tiles=[Image.open(OUT+n+".png") for n in names]
    for k,(a,b) in enumerate([(0,4),(4,8)]):
        sheet=Image.new("RGB",(1560,520),(245,235,221))
        x=10
        for t in tiles[a:b]:
            sheet.paste(t,(x,10),t); x+=t.width+10
        sheet.save(OUT+"preview%d.png"%k)
