import cv2, numpy as np
from PIL import Image
U='/root/.claude/uploads/8cf8e492-a911-56d3-bda5-bd8d5647301e/'
def cut(path,seeds,tol,grow=1,blur=1.0):
    im=cv2.imread(path); h,w=im.shape[:2]
    mask=np.zeros((h+2,w+2),np.uint8)
    for (x,y) in seeds:
        x=min(max(x,0),w-1); y=min(max(y,0),h-1)
        cv2.floodFill(im.copy(),mask,(x,y),0,(tol,)*3,(tol,)*3,4|cv2.FLOODFILL_MASK_ONLY|cv2.FLOODFILL_FIXED_RANGE|(255<<8))
    bg=(mask[1:-1,1:-1]==255).astype(np.uint8)
    if grow: bg=cv2.dilate(bg,np.ones((3,3),np.uint8),iterations=grow)
    a=(1-bg).astype(np.float32)
    a=cv2.GaussianBlur(a,(0,0),blur)
    rgb=cv2.cvtColor(im,cv2.COLOR_BGR2RGB)
    out=np.dstack([rgb,(a*255).astype(np.uint8)])
    return Image.fromarray(out,'RGBA')
