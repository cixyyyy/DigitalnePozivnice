import math, random, sys
sys.argv=[sys.argv[0]]
from itsvg import *
from itscene import peony
Q=random.Random(21)
LILDEFS='''<defs>
<linearGradient id="ol" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#B2C294"/><stop offset="1" stop-color="#6E8A58"/></linearGradient>
<linearGradient id="ol2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#9DB07C"/><stop offset="1" stop-color="#5C7A4C"/></linearGradient>
<linearGradient id="stn" x1="0" y1="0" x2="1" y2="0"><stop offset="0" stop-color="#EBDDBF"/><stop offset=".5" stop-color="#FAF2DE"/><stop offset="1" stop-color="#DCCBA6"/></linearGradient>
<linearGradient id="rib" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#E7B2C6"/><stop offset=".5" stop-color="#F5D3DF"/><stop offset="1" stop-color="#CF8FAA"/></linearGradient>
<radialGradient id="pet" cx=".5" cy=".75" r=".8"><stop offset="0" stop-color="#FBE3E6"/><stop offset="1" stop-color="#F2AEBE"/></radialGradient>
</defs>'''
def lleaf(x,y,a,L,W): return leaf(x,y,a,L,W,'ol' if Q.random()<.5 else 'ol2')

def lavender(x,y,ang,L,fl=None):
    a=math.radians(ang); out=''
    ex,ey=x+L*math.cos(a),y+L*math.sin(a)
    mx,my=(x+ex)/2+Q.uniform(-14,14),(y+ey)/2+Q.uniform(-14,14)
    out+=f'<path d="M{x:.0f},{y:.0f} Q{mx:.0f},{my:.0f} {ex:.0f},{ey:.0f}" stroke="#7E8F62" stroke-width="3" fill="none" stroke-linecap="round"/>'
    n=11
    for i in range(n):
        t=.42+.58*i/(n-1)
        px=(1-t)**2*x+2*(1-t)*t*mx+t*t*ex; py=(1-t)**2*y+2*(1-t)*t*my+t*t*ey
        sz=1-.5*(i/(n-1))
        col=['#6F55A8','#8468BB','#9B82CC','#B29AD9'][min(3,i*4//n)]
        for s in (-1,1):
            r=ang+s*38
            out+=f'<ellipse cx="0" cy="-10" rx="{6.3*sz:.1f}" ry="{12*sz:.1f}" transform="translate({px:.1f},{py:.1f}) rotate({r+90:.0f})" fill="{col}" stroke="#4F3A86" stroke-opacity=".35" stroke-width=".8"/>'
        out+=f'<circle cx="{px:.1f}" cy="{py:.1f}" r="2.4" fill="#E8B96A"/>'
    out+=f'<ellipse cx="0" cy="-8" rx="4.5" ry="11" transform="translate({ex:.1f},{ey:.1f}) rotate({ang+90:.0f})" fill="#B29AD9"/>'
    return out

def blossom5(x,y,r,rot=0):
    g=f'<g transform="translate({x:.1f},{y:.1f}) rotate({rot})">'
    for j in range(5):
        g+=f'<path d="M0,0 C{-r*.55:.1f},{-r*.35:.1f} {-r*.62:.1f},{-r*.95:.1f} {-r*.14:.1f},{-r*1.0:.1f} L0,{-r*.86:.1f} L{r*.14:.1f},{-r*1.0:.1f} C{r*.62:.1f},{-r*.95:.1f} {r*.55:.1f},{-r*.35:.1f} 0,0Z" transform="rotate({j*72})" fill="url(#pet)" stroke="#E593A8" stroke-opacity=".6" stroke-width=".9"/>'
        g+=f'<path d="M0,{-r*.15:.1f} L{-r*.12:.1f},{-r*.7:.1f} M0,{-r*.15:.1f} L{r*.1:.1f},{-r*.72:.1f}" transform="rotate({j*72})" stroke="#E27F99" stroke-opacity=".55" stroke-width="1.2" fill="none"/>'
    for k in range(14):
        a=k*math.pi*2/14; g+=f'<line x1="0" y1="0" x2="{r*.34*math.cos(a):.1f}" y2="{r*.34*math.sin(a):.1f}" stroke="#D9A24E" stroke-width=".9"/><circle cx="{r*.34*math.cos(a):.1f}" cy="{r*.34*math.sin(a):.1f}" r="1.5" fill="#E7B65C"/>'
    return g+f'<circle r="{r*.1:.1f}" fill="#F7E3A4"/></g>'

def bud(x,y,ang,L):
    return (f'<g transform="translate({x:.1f},{y:.1f}) rotate({ang})"><path d="M0,0 C{L*.2:.1f},{-L*.3:.1f} {L*.55:.1f},{-L*.38:.1f} {L:.1f},0 C{L*.55:.1f},{L*.38:.1f} {L*.2:.1f},{L*.3:.1f} 0,0Z" fill="#F4B8C6" stroke="#DB8DA3" stroke-width=".9"/>'
            f'<path d="M0,0 C{L*.12:.1f},{-L*.2:.1f} {L*.4:.1f},{-L*.14:.1f} {L*.7:.1f},0" stroke="#fff" stroke-opacity=".5" stroke-width="2" fill="none"/><path d="M-2,0 q8,-10 14,0 q-6,10 -14,0Z" fill="#9AAE7E"/></g>')

def twig(pts,flowers=True):
    out=stem(pts,2.4,'#8A7A56')
    return out

def bouquet_l(W=760,H=720):
    b=''
    base=(60,640)
    # leaves
    for (a,L,Wd) in [(-20,170,36),(-50,190,36),(-80,170,34),(-8,150,30),(-35,140,32)]:
        b+=lleaf(base[0]+Q.uniform(-10,30),base[1]+Q.uniform(-30,10),a+Q.uniform(-6,6),L,Wd)
    # lavender spikes
    for (a,L) in [(-64,470),(-75,430),(-52,420),(-84,380),(-44,360)]:
        b+=lavender(base[0]+Q.uniform(0,40),base[1]-Q.uniform(0,40),a,L)
    # blossom branch
    p=bez((70,630),(170,560),(260,470),(380,420),16)
    b+=stem(p,3,'#8A7A56')
    for (x,y,r,rot) in [(300,455,66,10),(150,540,40,-20),(420,415,32,30)]: b+=blossom5(x,y,r,rot)
    b+=bud(380,424,-35,44)+bud(430,420,-25,36)+bud(222,490,-70,40)+bud(120,500,-120,34)
    for (x,y,a) in [(200,520,-10),(340,452,20),(110,580,-40)]: b+=lleaf(x,y,a,70,18)
    return b

def wisteria(x,y,Ln=200,Wd=70):
    b=stem([(x,y),(x+Q.uniform(-4,4),y+Ln)],2.2,'#7E8F62')
    for i in range(int(Ln/9)):
        t=i/(Ln/9); wd=Wd*(1-t)**0.8*.5+4
        for s in (-1,1):
            px=x+s*wd*Q.uniform(.2,1); py=y+i*9+Q.uniform(-3,3)
            col=['#C3A9E0','#B193D6','#9D7FC9','#8B6BBD'][Q.randint(0,3)]
            b+=f'<ellipse cx="{px:.1f}" cy="{py:.1f}" rx="6.5" ry="9" transform="rotate({Q.uniform(-25,25):.0f} {px:.1f} {py:.1f})" fill="{col}" stroke="#6F55A8" stroke-opacity=".3" stroke-width=".7"/>'
    return b

def peonyp(x,y,r): return peony(x,y,r,'#FFF1F0','#F6C4CC')

def ribbon_path(pts,w0=22,w1=22,wob=0):
    return f'<path d="{taper(pts,w0,w1,1)}" fill="url(#rib)" stroke="#CF8FAA" stroke-opacity=".5" stroke-width="1"/>'

def bow_pink(cx,cy,s=1):
    g=f'<g transform="translate({cx},{cy}) scale({s})">'
    g+='<path d="M0,0 C-60,-52 -150,-34 -120,14 C-96,44 -34,26 0,0Z" fill="url(#rib)" stroke="#CF8FAA" stroke-opacity=".6"/>'
    g+='<path d="M0,0 C60,-52 150,-34 120,14 C96,44 34,26 0,0Z" fill="url(#rib)" stroke="#CF8FAA" stroke-opacity=".6"/>'
    g+='<path d="M-8,-4 C-56,-30 -100,-18 -96,6" stroke="#fff" stroke-opacity=".5" stroke-width="3" fill="none"/><path d="M8,-4 C56,-30 100,-18 96,6" stroke="#fff" stroke-opacity=".5" stroke-width="3" fill="none"/>'
    g+='<path d="M-6,8 C-30,50 -60,82 -86,112 L-62,108 C-44,86 -20,56 6,16Z" fill="#E7B2C6"/><path d="M6,8 C30,50 60,82 86,112 L62,108 C44,86 20,56 -6,16Z" fill="#E7B2C6"/>'
    g+='<ellipse cx="0" cy="2" rx="17" ry="14" fill="#E1A0B9" stroke="#CF8FAA"/>'
    g+='<path d="M0,22 L11,40 L0,60 L-11,40Z" fill="#E3C27A" stroke="#B8955A"/><circle cy="40" r="3" fill="#FFF4CF"/></g>'
    return g

def gold_orn(cx,cy):
    g=f'<g transform="translate({cx},{cy})">'
    for s in (-1,1):
        g+=f'<g transform="scale({s},1)"><path d="M0,0 C20,-18 44,-18 60,-4 C74,8 92,6 100,-6 C94,-2 82,-2 74,-10 C60,-26 28,-30 4,-8Z" fill="#CDAE6A"/><path d="M60,-4 C70,-24 90,-30 110,-18 C94,-20 82,-14 76,-4Z" fill="#B8955A"/></g>'
    g+='<path d="M0,-18 L9,-4 L0,10 L-9,-4Z" fill="#D9BC7A" stroke="#B8955A"/></g>'
    return g

def svgl(w,h,body,flt=True):
    g=f'<g filter="url(#paint)">{body}</g>' if flt else body
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">{DEFS}{LILDEFS}{g}</svg>'

W_=lambda s:s
# 1) bouquet (left version); right = CSS mirror
open(OUT+'/lb_bouq.svg','w').write(svgl(760,720,bouquet_l()))

# 2) frame top 900x340
def corner_cluster(flip=False):
    b=''
    for (x,y,a,L,Wd) in [(40,170,-20,110,26),(80,210,10,110,26),(130,60,-60,100,24),(20,100,-80,90,22)]: b+=lleaf(x,y,a,L,Wd)
    b+=wisteria(62,60,170,56)+wisteria(150,110,120,40)+wisteria(24,150,100,30)
    b+=peonyp(120,130,52)+blossom5(210,92,28,10)+blossom5(60,200,26,-12)
    b+=bud(190,60,-30,28)
    t=f'translate(900,0) scale(-1,1)' if flip else ''
    return f'<g transform="{t}">{b}</g>'
def frame_top():
    b=''
    # arch (stone)
    arch=bez((90,340),(90,120),(300,36),(450,36),30)+bez((450,36),(600,36),(810,120),(810,340),30)[1:]
    b+=f'<path d="{taper(arch,34,34)}" fill="url(#stn)" stroke="#C8B48A" stroke-width="1.2"/>'
    b+=f'<path d="{taper([(x,y+0) for x,y in arch],10,10)}" fill="#fff" opacity=".25"/>'
    # ribbon waving along arch
    rb=[(x+8*math.sin(i*.55),y+10*math.cos(i*.55)) for i,(x,y) in enumerate(arch)]
    b+=ribbon_path(rb,26,26)
    # corners
    b+=corner_cluster(False)+corner_cluster(True)
    b+=bow_pink(450,40,1.0)
    for (x,y) in [(250,120),(640,100),(320,70),(580,64)]: b+=f'<path d="M{x},{y} q8,-14 16,0 q-8,12 -16,0Z" fill="#F2B6C4" opacity=".8"/>'
    return svgl(900,340,b)
open(OUT+'/lb_frame_top.svg','w').write(frame_top())

def frame_bot():
    b=''
    for x in (100,800):
        b+=f'<rect x="{x-34}" y="70" width="68" height="180" rx="3" fill="url(#stn)" stroke="#C8B48A"/><rect x="{x-44}" y="52" width="88" height="24" rx="3" fill="url(#stn)" stroke="#C8B48A"/><rect x="{x-44}" y="236" width="88" height="22" rx="3" fill="url(#stn)" stroke="#C8B48A"/>'
        for k in (-18,-6,6,18): b+=f'<line x1="{x+k}" y1="82" x2="{x+k}" y2="232" stroke="#CDBB93" stroke-width="2" opacity=".7"/>'
    # clusters
    def cl(flip):
        s=''
        for (x,y,a,L,Wd) in [(50,150,-25,110,26),(110,200,0,110,26),(30,240,-10,100,24),(140,100,-60,90,22)]: s+=lleaf(x,y,a,L,Wd)
        s+=wisteria(70,10,150,50)+wisteria(140,20,110,36)
        s+=peonyp(110,160,50)+blossom5(200,120,26,10)+blossom5(46,214,26,-12)+bud(180,200,-10,28)
        return f'<g transform="translate(900,0) scale(-1,1)">{s}</g>' if flip else f'<g>{s}</g>'
    b+=cl(False)+cl(True)+gold_orn(450,246)
    return svgl(900,300,b)
open(OUT+'/lb_frame_bot.svg','w').write(frame_bot())

def frame_side(flip):
    b=f'<rect x="38" y="0" width="64" height="240" fill="url(#stn)" stroke="#C8B48A"/>'
    for k in (50,62,74,86): b+=f'<line x1="{k}" y1="0" x2="{k}" y2="240" stroke="#CDBB93" stroke-width="2" opacity=".7"/>'
    # twisting ribbon + a hanging wisteria bit
    b+=wisteria(36,60,110,26)
    b+=lleaf(30,40,-120,40,10)+lleaf(34,150,140,40,10)+blossom5(30,110,14,0)
    t=f'translate(140,0) scale(-1,1)' if flip else ''
    return svgl(140,240,f'<g transform="{t}">{b}</g>',flt=False)
open(OUT+'/lb_side_l.svg','w').write(frame_side(False)); open(OUT+'/lb_side_r.svg','w').write(frame_side(True))

# 3) footer scene 1600x660
def footer():
    W,H=1600,660
    b='<defs><linearGradient id="bgf" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F8F3FA" stop-opacity="0"/><stop offset=".45" stop-color="#F6E9F1"/><stop offset="1" stop-color="#F0D9E3"/></linearGradient><linearGradient id="st1" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F4E4DA"/><stop offset="1" stop-color="#E2C7BA"/></linearGradient></defs>'
    b+=f'<rect width="{W}" height="{H}" fill="url(#bgf)"/>'
    # steps (three trapezoid bands)
    ys=[360,450,540,640]
    for i in range(3):
        inset=120-i*40
        b+=f'<path d="M{inset},{ys[i]} L{W-inset},{ys[i]} L{W-inset+40},{ys[i+1]} L{inset-40},{ys[i+1]}Z" fill="url(#st1)" stroke="#D9BBAA" stroke-width="2"/>'
        b+=f'<rect x="{inset-40}" y="{ys[i+1]-12}" width="{W-2*inset+80}" height="14" fill="#E8D2C5" opacity=".8"/>'
    for _ in range(46):
        x=Q.uniform(160,W-160); y=Q.uniform(380,630); p=[(x,y)]
        for k in range(Q.randint(3,6)): x+=Q.uniform(-26,26); y+=Q.uniform(-14,18); p.append((x,y))
        b+='<path d="M'+' L'.join(f'{u:.0f},{v:.0f}' for u,v in p)+'" stroke="#D8B77A" stroke-opacity=".5" stroke-width="1.3" fill="none"/>'
    # pedestals
    for x in (230,1370):
        b+=f'<rect x="{x-70}" y="250" width="140" height="170" rx="4" fill="url(#stn)" stroke="#C8B48A"/><rect x="{x-84}" y="232" width="168" height="26" rx="3" fill="url(#stn)" stroke="#C8B48A"/><rect x="{x-84}" y="408" width="168" height="26" rx="3" fill="url(#stn)" stroke="#C8B48A"/>'
    # bouquets both sides
    def side(flip):
        s=''
        for (x,y,a,L,Wd) in [(40,430,-40,200,40),(120,470,-10,200,40),(20,320,-70,170,36),(220,420,-30,170,36),(60,520,5,170,34)]: s+=lleaf(x,y,a,L,Wd)
        s+=wisteria(60,150,230,70)+wisteria(150,170,200,64)+wisteria(250,200,160,50)
        s+=peonyp(130,330,90)+peonyp(300,360,70)+blossom5(210,260,44,10)+blossom5(60,420,40,-10)+blossom5(330,300,34,20)
        for (x,y) in [(250,420),(40,300),(180,400),(340,420)]: s+=bud(x,y,Q.uniform(-70,-20),34)
        s+=lavender(120,500,-70,260)+lavender(200,510,-55,230)
        return f'<g transform="translate({W},0) scale(-1,1)">{s}</g>' if flip else f'<g>{s}</g>'
    b+=side(False)+side(True)
    # ribbons swirling
    r1=bez((330,470),(520,380),(560,560),(760,600),30)
    b+=ribbon_path(r1,26,10)
    r2=bez((1270,470),(1080,380),(1040,570),(840,605),30)
    b+=ribbon_path(r2,26,10)
    for _ in range(18):
        x,y=Q.uniform(260,1340),Q.uniform(560,640)
        b+=f'<path d="M{x:.0f},{y:.0f} q9,-16 18,0 q-9,14 -18,0Z" fill="#F2B6C4" opacity=".85" transform="rotate({Q.uniform(-30,30):.0f} {x:.0f} {y:.0f})"/>'
    return svgl(W,H,b)
open(OUT+'/lb_footer.svg','w').write(footer())
print('ok')
