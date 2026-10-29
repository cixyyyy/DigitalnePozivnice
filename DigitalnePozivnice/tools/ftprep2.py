import json, numpy as np
from PIL import Image
OUT="/home/claude/W/wwwroot/img/fotomat/"
S="/tmp/claude-0/-home-claude/8cf8e492-a911-56d3-bda5-bd8d5647301e/scratchpad/ft/"
# kamera: odsijeci tamnu sjenu ispod, sacuvaj 1x
c=Image.open(S+"camera_clean.png").convert("RGBA")
a=np.array(c); H=1018
a=a[:H]; ys=np.where(a[...,3]>40)[0]; print("cam h",a.shape, ys.max())
Image.fromarray(a,"RGBA").save(OUT+"camera.webp",quality=90,method=6)
g=json.load(open("/tmp/ft_cam.json")); w=a.shape[1]
geo={"aspect":round(w/H,4),"slot":[g["slot"][0],round(g["slot"][1]*1111/H,2),g["slot"][2],round(g["slot"][3]*1111/H,2)],
     "button":[round(186/w*100,2),round(304/H*100,2),round(104/w*100,2)],"flash":[9.8,7.4,19.5,10.8]}
print(geo); json.dump(geo,open("/tmp/ft_cam2.json","w"))
# ukrasi (zlatni), umanjeni
for name,src,hh in [("orn-1","o3",300),("orn-2","o4",260),("orn-3","o5",420),("orn-4","o6",420),("orn-5","o1",420),("orn-6","o2",420)]:
    im=Image.open(S+src+".png").convert("RGBA"); r=hh/im.height
    im=im.resize((max(1,int(im.width*r)),hh),Image.LANCZOS); im.save(OUT+name+".webp",quality=86,method=6); print(name,im.size)
