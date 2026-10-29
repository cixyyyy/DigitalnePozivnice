import math
W,H=600,860
CX,CY=300,430
def mirror(pts):
    return pts
# desna polovica konture (od vrha prema dnu), pa se odrazi
right = [
 ("M",(300,34)),
 ("C",(338,34),(352,64),(396,66)),
 ("C",(446,68),(468,40),(520,72)),
 ("C",(562,100),(578,152),(566,204)),
 ("C",(556,250),(541,300),(541,430)),
 ("C",(541,560),(556,610),(566,656)),
 ("C",(578,708),(562,760),(520,788)),
 ("C",(468,820),(446,792),(396,794)),
 ("C",(352,796),(338,826),(300,826)),
]
def mir(p): return (2*CX-p[0],p[1])
def path():
    d=""
    for seg in right:
        if seg[0]=="M": d+="M%g,%g"%seg[1]
        else: d+="C%g,%g %g,%g %g,%g"%(seg[1]+seg[2]+seg[3])
    # lijeva polovica: obrnut redoslijed
    pts=[]
    cur=right[0][1]
    segs=[]
    for seg in right[1:]:
        segs.append((cur,seg[1],seg[2],seg[3])); cur=seg[3]
    for (p0,c1,c2,p1) in reversed(segs):
        d+="C%g,%g %g,%g %g,%g"%(mir(c2)+mir(c1)+mir(p0))
    return d+"Z"
D=path()
def sc(s): return 'transform="translate(%g %g) scale(%g) translate(%g %g)"'%(CX,CY,s,-CX,-CY)
def spiral(cx,cy,r0,turns,rot,flip=1,n=60,k=0.22):
    pts=[]
    for i in range(n+1):
        t=i/n*turns*2*math.pi
        r=r0*math.exp(-k*t/ (2*math.pi) *1.6)
        a=rot+flip*t
        pts.append((cx+r*math.cos(a),cy+r*math.sin(a)))
    return "M"+" L".join("%.1f,%.1f"%p for p in pts)
def leaf(x,y,ang,l,w):
    # list (tear drop) rotated
    return ('<path d="M0,0 C%g,%g %g,%g %g,0 C%g,%g %g,%g 0,0Z" transform="translate(%g %g) rotate(%g)"/>'
            %(l*.25,-w,l*.75,-w,l,l*.75,w,l*.25,w,x,y,ang))
def fan(x,y,ang,n,span,l,w):
    out=""
    for i in range(n):
        a=ang-span/2+span*i/(n-1)
        out+=leaf(x,y,a,l,w)
    return out

svg=[]
svg.append('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 %d %d">'%(W,H))
svg.append('''<defs>
<linearGradient id="g1" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#f6e3a4"/><stop offset=".28" stop-color="#d9b25f"/><stop offset=".55" stop-color="#9b7430"/><stop offset=".78" stop-color="#e2c070"/><stop offset="1" stop-color="#8a6526"/></linearGradient>
<linearGradient id="g2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff0b8"/><stop offset=".35" stop-color="#e6c377"/><stop offset=".7" stop-color="#b58a3d"/><stop offset="1" stop-color="#f0d38c"/></linearGradient>
<linearGradient id="g3" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#7d5a22"/><stop offset=".5" stop-color="#c9a24f"/><stop offset="1" stop-color="#7d5a22"/></linearGradient>
<radialGradient id="pan" cx=".5" cy=".42" r=".75"><stop offset="0" stop-color="#e7cf9a"/><stop offset=".6" stop-color="#d6b97c"/><stop offset="1" stop-color="#b99650"/></radialGradient>
<filter id="sh" x="-10%" y="-10%" width="120%" height="120%"><feDropShadow dx="0" dy="6" stdDeviation="7" flood-color="#5b4318" flood-opacity=".38"/></filter>
<filter id="grain" x="0" y="0" width="100%" height="100%"><feTurbulence type="fractalNoise" baseFrequency=".9" numOctaves="2" seed="4"/><feColorMatrix values="0 0 0 0 .45  0 0 0 0 .33  0 0 0 0 .12  0 0 0 .55 0"/><feComposite in2="SourceGraphic" operator="in"/></filter>
<filter id="bl" x="-20%" y="-20%" width="140%" height="140%"><feGaussianBlur stdDeviation="9"/></filter>
<filter id="bev"><feGaussianBlur in="SourceAlpha" stdDeviation="1.6" result="b"/><feSpecularLighting in="b" surfaceScale="4" specularConstant=".9" specularExponent="22" lighting-color="#fff6d6" result="s"><feDistantLight azimuth="235" elevation="48"/></feSpecularLighting><feComposite in="s" in2="SourceAlpha" operator="in" result="s2"/><feComposite in="SourceGraphic" in2="s2" operator="arithmetic" k1="0" k2="1" k3=".75" k4="0"/></filter>
<clipPath id="inner"><path d="@D@" @S@/></clipPath>
</defs>'''.replace("@D@",D).replace("@S@",sc(0.86)))
svg.append('<g filter="url(#sh)"><path d="%s" fill="url(#g1)"/></g>'%D)
svg.append('<path d="%s" %s fill="url(#g2)"/>'%(D,sc(0.985)))
svg.append('<path d="%s" %s fill="url(#g3)"/>'%(D,sc(0.915)))
svg.append('<path d="%s" %s fill="url(#g1)"/>'%(D,sc(0.905)))
# perlice na srednjoj stazi
svg.append('<path d="%s" %s fill="none" stroke="url(#g2)" stroke-width="7" stroke-linecap="round" stroke-dasharray="0 13"/>'%(D,sc(0.945)))
svg.append('<path d="%s" %s fill="none" stroke="#7a5a22" stroke-width="1.2" opacity=".7"/>'%(D,sc(0.968)))
# panel
svg.append('<path d="%s" %s fill="url(#pan)"/>'%(D,sc(0.86)))
svg.append('<g clip-path="url(#inner)"><path d="%s" %s fill="#fff" filter="url(#grain)" opacity=".45"/>'%(D,sc(0.86)))
svg.append('<path d="%s" %s fill="none" stroke="#6b4c18" stroke-width="22" opacity=".55" filter="url(#bl)"/></g>'%(D,sc(0.86)))
svg.append('<path d="%s" %s fill="none" stroke="#f3dc9d" stroke-width="2.2"/>'%(D,sc(0.862)))
# ukrasi (zlatni svici i lisce) – gornji dio, odrazen na donji i lijevi
orn=[]
def side(sg):
    # sg=1 desno
    X=lambda x: CX+sg*(x-CX)
    s=[]
    for flip in (1,):
        pass
    return s
def corner(x,y,rot,flip):
    g=""
    g+='<path d="%s" fill="none" stroke="url(#g3)" stroke-width="12" stroke-linecap="round"/>'%spiral(x,y,34,1.55,rot,flip)
    g+='<path d="%s" fill="none" stroke="url(#g2)" stroke-width="7" stroke-linecap="round"/>'%spiral(x,y,34,1.55,rot,flip)
    g+='<path d="%s" fill="none" stroke="#fff3c4" stroke-width="1.6" stroke-linecap="round" opacity=".8" transform="translate(-1.5 -1.5)"/>'%spiral(x,y,34,1.55,rot,flip)
    return g
G=[]
# lisce i svici uz vrh (desno + lijevo odraz)
def top_half():
    g=""
    g+='<g fill="url(#g2)" stroke="#7d5a22" stroke-width="1">'+fan(300,70,-90,7,150,52,13)+'</g>'
    g+='<g fill="url(#g1)" stroke="#6b4c18" stroke-width="1">'+fan(300,76,-90,5,100,34,9)+'</g>'
    g+='<circle cx="300" cy="72" r="11" fill="url(#g2)" stroke="#6b4c18"/><circle cx="300" cy="72" r="4.5" fill="#8a6526"/>'
    g+=corner(432,96,math.radians(-40),1)
    g+='<g fill="url(#g2)" stroke="#7d5a22" stroke-width="1">'+leaf(372,100,200,44,11)+leaf(470,122,30,40,10)+leaf(508,160,60,34,9)+'</g>'
    g+='<g fill="url(#g2)" stroke="#6b4c18"><circle cx="438" cy="102" r="7"/><circle cx="535" cy="150" r="6"/></g>'
    return g
def mirror_x(g): return '<g transform="translate(600 0) scale(-1 1)">%s</g>'%g
def mirror_y(g): return '<g transform="translate(0 %d) scale(1 -1)">%s</g>'%(H,g)
t=top_half()
tl=mirror_x(t)
top=t+tl
svg.append(top)
svg.append(mirror_y(top))
# bocni mali svici
def mid(x,y):
    return ('<g fill="url(#g2)" stroke="#6b4c18" stroke-width="1"><ellipse cx="%g" cy="%g" rx="10" ry="16"/></g>'%(x,y))
svg.append('</svg>')
open("/home/claude/W/wwwroot/img/fotomat/okvir.svg","w").write("\n".join(svg))
print("ok",len("\n".join(svg)))
