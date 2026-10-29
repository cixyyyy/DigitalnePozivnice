import math,random
OUT="/home/claude/W/wwwroot/img/fotomat/"
def heart_pts(n,scale,cx,cy):
    pts=[]
    for i in range(n):
        t=2*math.pi*i/n
        x=16*math.sin(t)**3
        y=-(13*math.cos(t)-5*math.cos(2*t)-2*math.cos(3*t)-math.cos(4*t))
        pts.append((cx+x*scale,cy+y*scale))
    return pts
def resample(pts,n):
    import itertools
    pts=pts+[pts[0]]
    d=[0]
    for a,b in zip(pts,pts[1:]): d.append(d[-1]+math.hypot(b[0]-a[0],b[1]-a[1]))
    L=d[-1]; out=[]; j=0
    for i in range(n):
        t=L*i/n
        while d[j+1]<t: j+=1
        f=(t-d[j])/(d[j+1]-d[j]); a,b=pts[j],pts[j+1]
        out.append((a[0]+(b[0]-a[0])*f,a[1]+(b[1]-a[1])*f))
    return out
def poly(pts): return "M"+" L".join("%.1f,%.1f"%p for p in pts)+"Z"
def volan_srce():
    W,H=440,440
    cx,cy=220,214
    outer=resample(heart_pts(900,9.9,cx,cy),40)
    inner=heart_pts(160,8.1,cx,cy)
    s='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d"><defs>'%(W,H)
    s+='<filter id="sh" x="-15%" y="-15%" width="130%" height="140%"><feDropShadow dx="0" dy="5" stdDeviation="6" flood-color="#7a6030" flood-opacity=".35"/></filter>'
    s+='<radialGradient id="c" cx=".4" cy=".35" r=".8"><stop offset="0" stop-color="#fffaf0"/><stop offset="1" stop-color="#efe0c2"/></radialGradient>'
    s+='<radialGradient id="f" cx=".5" cy=".4" r=".7"><stop offset="0" stop-color="#fdf4e1"/><stop offset="1" stop-color="#ead9b6"/></radialGradient></defs>'
    s+='<g filter="url(#sh)">'
    for (x,y) in outer:
        s+='<circle cx="%.1f" cy="%.1f" r="19.5" fill="url(#c)" stroke="#d9c597" stroke-width="1.4"/>'%(x,y)
    s+='</g>'
    for (x,y) in outer:
        s+='<circle cx="%.1f" cy="%.1f" r="11" fill="none" stroke="#e6d6b0" stroke-width="3" opacity=".7"/>'%(x,y)
    s+='<path d="%s" fill="url(#f)" stroke="#d9c597" stroke-width="1.5"/>'%poly(inner)
    s+='<path d="%s" fill="none" stroke="#e3d1a6" stroke-width="1" stroke-dasharray="2 5" transform="translate(%g %g) scale(.9) translate(%g %g)"/>'%(poly(heart_pts(160,8.1,cx,cy)),cx,cy,-cx,-cy)
    s+='</svg>'
    open(OUT+"srce-volan.svg","w").write(s)
def volan_krug():
    W=H=400; cx=cy=200
    s='<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d"><defs>'%(W,H)
    s+='<filter id="sh" x="-15%" y="-15%" width="130%" height="140%"><feDropShadow dx="0" dy="5" stdDeviation="6" flood-color="#7a6030" flood-opacity=".35"/></filter>'
    s+='<radialGradient id="c" cx=".4" cy=".35" r=".8"><stop offset="0" stop-color="#fffaf0"/><stop offset="1" stop-color="#efe0c2"/></radialGradient>'
    s+='<radialGradient id="f" cx=".5" cy=".4" r=".7"><stop offset="0" stop-color="#fdf4e1"/><stop offset="1" stop-color="#ead9b6"/></radialGradient></defs><g filter="url(#sh)">'
    n=34; R=168
    for i in range(n):
        a=2*math.pi*i/n
        s+='<circle cx="%.1f" cy="%.1f" r="23" fill="url(#c)" stroke="#d9c597" stroke-width="1.4"/>'%(cx+R*math.cos(a),cy+R*math.sin(a))
    s+='</g><circle cx="200" cy="200" r="158" fill="url(#f)" stroke="#d9c597" stroke-width="1.5"/>'
    s+='<circle cx="200" cy="200" r="146" fill="none" stroke="#e3d1a6" stroke-width="1" stroke-dasharray="2 5"/></svg>'
    open(OUT+"krug-volan.svg","w").write(s)
volan_srce(); volan_krug(); print("ok")
