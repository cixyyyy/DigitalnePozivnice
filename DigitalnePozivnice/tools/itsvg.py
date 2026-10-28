import math, random, os
R=random.Random(7)
OUT='/home/claude/art_it'; os.makedirs(OUT,exist_ok=True)

DEFS='''<defs>
<radialGradient id="lem" cx=".38" cy=".32" r=".8"><stop offset="0" stop-color="#FBEB8E"/><stop offset=".45" stop-color="#F2CB3C"/><stop offset="1" stop-color="#C98F0E"/></radialGradient>
<linearGradient id="lf" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#8FB06A"/><stop offset="1" stop-color="#3F6B3B"/></linearGradient>
<linearGradient id="lf2" x1="0" y1="1" x2="1" y2="0"><stop offset="0" stop-color="#6E9558"/><stop offset="1" stop-color="#2F5A36"/></linearGradient>
<filter id="paint" x="-5%" y="-5%" width="110%" height="110%">
 <feTurbulence type="fractalNoise" baseFrequency=".035" numOctaves="3" seed="3" result="n"/>
 <feDisplacementMap in="SourceGraphic" in2="n" scale="4" result="d"/>
 <feTurbulence type="fractalNoise" baseFrequency=".9" numOctaves="2" seed="9" result="g"/>
 <feColorMatrix in="g" type="matrix" values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 -1.1 1.05" result="ga"/>
 <feComposite in="d" in2="ga" operator="in" result="gr"/>
 <feMerge><feMergeNode in="d"/><feMergeNode in="gr"/></feMerge>
</filter>
<filter id="soft"><feGaussianBlur stdDeviation="1.1"/></filter>
</defs>'''

def leaf(x,y,ang,L,W,grad='lf'):
    # pointed leaf with midrib
    a=math.radians(ang)
    c,s=math.cos(a),math.sin(a)
    def P(u,v): return (x+u*c-v*s, y+u*s+v*c)
    p0=P(0,0); p1=P(L*.3,-W); p2=P(L*.75,-W*.7); p3=P(L,0); p4=P(L*.75,W*.8); p5=P(L*.3,W*1.05)
    d=f'M{p0[0]:.1f},{p0[1]:.1f} C{p1[0]:.1f},{p1[1]:.1f} {p2[0]:.1f},{p2[1]:.1f} {p3[0]:.1f},{p3[1]:.1f} C{p4[0]:.1f},{p4[1]:.1f} {p5[0]:.1f},{p5[1]:.1f} {p0[0]:.1f},{p0[1]:.1f}Z'
    m=P(L*.92,0)
    hl=P(L*.5,-W*.45)
    return (f'<path d="{d}" fill="url(#{grad})" stroke="#2B4F30" stroke-opacity=".35" stroke-width="1"/>'
            f'<path d="M{p0[0]:.1f},{p0[1]:.1f} L{m[0]:.1f},{m[1]:.1f}" stroke="#E5EFC8" stroke-opacity=".55" stroke-width="1.3" fill="none"/>'
            f'<path d="M{p0[0]:.1f},{p0[1]:.1f} Q{hl[0]:.1f},{hl[1]:.1f} {P(L*.8,-W*.3)[0]:.1f},{P(L*.8,-W*.3)[1]:.1f}" stroke="#C9DDA0" stroke-opacity=".45" stroke-width="3" fill="none" stroke-linecap="round"/>')

def lemon(x,y,ang=0,s=1.0):
    return (f'<g transform="translate({x},{y}) rotate({ang}) scale({s})">'
      '<path d="M-52,6 C-52,-34 -18,-48 6,-46 C30,-46 52,-30 58,-4 C62,4 66,8 68,10 C62,14 58,16 54,22 C40,50 6,56 -22,46 C-44,38 -52,24 -52,6Z" fill="url(#lem)" stroke="#B07F0B" stroke-opacity=".55" stroke-width="1.4"/>'
      '<path d="M-40,22 C-30,46 4,52 30,42 C46,36 54,24 56,20 C40,40 6,48 -20,40 C-30,36 -38,30 -40,22Z" fill="#B67C08" opacity=".35"/>'
      '<ellipse cx="-22" cy="-18" rx="17" ry="9" fill="#FFF8C6" opacity=".7" transform="rotate(-24 -22 -18)"/>'
      '<circle cx="66" cy="10" r="3.4" fill="#A97508" opacity=".7"/>'
      + ''.join(f'<circle cx="{R.uniform(-44,50):.1f}" cy="{R.uniform(-30,40):.1f}" r="{R.uniform(.6,1.3):.1f}" fill="#A87408" opacity=".28"/>' for _ in range(34))
      +'</g>')

def blossom(x,y,s=1,rot=0):
    pet=''.join(f'<ellipse cx="0" cy="-11" rx="6.2" ry="11" transform="rotate({k*72})" fill="#FFFFFF" stroke="#D9D2C4" stroke-width=".8"/>' for k in range(5))
    st=''.join(f'<line x1="0" y1="0" x2="{4*math.cos(k)}" y2="{4*math.sin(k)}" stroke="#E2B43A" stroke-width=".8"/>' for k in [0,1.2,2.4,3.6,4.8])
    return f'<g transform="translate({x},{y}) rotate({rot}) scale({s})">{pet}<circle r="4" fill="#F4D35E"/>{st}</g>'

def floret(x,y,r,col,rot):
    p=''.join(f'<ellipse cx="0" cy="{-r*.52:.1f}" rx="{r*.46:.1f}" ry="{r*.56:.1f}" transform="rotate({k*90+rot})" fill="{col}"/>' for k in range(4))
    hl=''.join(f'<ellipse cx="0" cy="{-r*.5:.1f}" rx="{r*.2:.1f}" ry="{r*.3:.1f}" transform="rotate({k*90+rot})" fill="#fff" opacity=".28"/>' for k in range(4))
    return f'<g transform="translate({x:.1f},{y:.1f})">{p}{hl}<circle r="{r*.14:.1f}" fill="#E9E2A8" opacity=".9"/></g>'

def hydrangea(cx,cy,rx,ry,pal,n=70,fr=17):
    pts=[]
    tries=0
    while len(pts)<n and tries<8000:
        tries+=1
        x=R.uniform(-1,1); y=R.uniform(-1,1)
        if x*x+y*y>1: continue
        X,Y=cx+x*rx,cy+y*ry
        if all((X-a)**2+(Y-b)**2>(fr*1.05)**2 for a,b in pts): pts.append((X,Y))
    pts.sort(key=lambda p:p[1])
    out=''
    for X,Y in pts:
        t=(Y-(cy-ry))/(2*ry)
        base=pal[min(len(pal)-1,int((t*.6+R.random()*.7)*len(pal)))]
        out+=floret(X,Y,fr*R.uniform(.9,1.15),base,R.uniform(0,90))
    return out

def stem(pts,w=5,col='#6B5A3A'):
    d='M'+' L'.join(f'{x:.1f},{y:.1f}' for x,y in pts)
    return f'<path d="{d}" fill="none" stroke="{col}" stroke-width="{w}" stroke-linecap="round" stroke-linejoin="round"/>'

def bez(p0,p1,p2,p3,n=24):
    return [tuple((1-t)**3*a+3*(1-t)**2*t*b+3*(1-t)*t*t*c+t**3*d for a,b,c,d in zip(p0,p1,p2,p3)) for t in [i/n for i in range(n+1)]]

def svg(w,h,body,flt=True):
    g=f'<g filter="url(#paint)">{body}</g>' if flt else body
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">{DEFS}{g}</svg>'

# ---------- 1) lemon branch (enters from top-right)
def lemons():
    b=''
    br=bez((600,-10),(500,40),(380,60),(250,150)); b+=stem(br,6)
    br2=bez((470,40),(440,110),(430,170),(400,250)); b+=stem(br2,4)
    br3=bez((330,95),(290,60),(250,40),(190,50)); b+=stem(br3,3.5)
    for (x,y,a,L,W,g) in [(520,22,160,120,34,'lf'),(430,60,135,110,30,'lf2'),(380,76,205,110,32,'lf'),(330,100,155,100,28,'lf2'),(262,138,200,100,30,'lf'),(470,50,60,90,26,'lf2'),(410,230,70,100,30,'lf'),(300,92,-110,80,24,'lf2'),(200,52,175,90,26,'lf')]:
        b+=leaf(x,y,a,L,W,g)
    b+=lemon(405,300,-18,1.15)+lemon(215,170,12,1.0)
    b+=blossom(520,100,.9,10)+blossom(345,40,.8,30)+blossom(300,230,.7,0)+blossom(150,80,.65,-20)
    return svg(600,430,b)

# ---------- 2) hydrangea + lemon cluster (bottom-left corner, anchored lower-left)
def hydr():
    b=''
    for pts in [bez((0,520),(80,470),(160,420),(260,330)),bez((40,560),(120,500),(200,470),(340,430))]: b+=stem(pts,5)
    for (x,y,a,L,W,g) in [(150,430,-70,150,40,'lf'),(250,400,-20,140,38,'lf2'),(80,380,-100,130,36,'lf2'),(300,470,-5,150,38,'lf'),(200,520,20,140,34,'lf2'),(330,360,-45,120,32,'lf'),(60,300,-95,110,32,'lf')]:
        b+=leaf(x,y,a,L,W,g)
    b+=hydrangea(190,330,150,125,['#B8C8EC','#9DB2E2','#7F99D3','#6682C2'],n=120,fr=21)
    b+=hydrangea(370,290,95,90,['#FFFFFF','#F6F3E9','#EAE6D6','#DDEBF2'],n=40,fr=19)
    b+=lemon(300,500,-8,.9)+lemon(430,445,18,.8)
    b+=blossom(90,470,.7,0)+blossom(140,250,.6,20)+blossom(420,370,.6,0)
    return svg(560,640,b)

# ---------- 3) blue baroque scrolls (Delft style)
BLUE='#2A4B8F'
def taper(pts,w0,w1,pw=1.0):
    n=len(pts); L=[];Rr=[]
    for i,(x,y) in enumerate(pts):
        x0,y0=pts[max(0,i-1)]; x1,y1=pts[min(n-1,i+1)]
        dx,dy=x1-x0,y1-y0; d=math.hypot(dx,dy) or 1
        nx,ny=-dy/d,dx/d
        t=i/(n-1); wd=(w0+(w1-w0)*(t**pw))/2
        L.append((x+nx*wd,y+ny*wd)); Rr.append((x-nx*wd,y-ny*wd))
    P=L+Rr[::-1]
    return 'M'+' L'.join(f'{x:.1f},{y:.1f}' for x,y in P)+'Z'

def spiral(R0,turns,steps=60,k=.86):
    pts=[]; pm=turns*2*math.pi
    for i in range(steps+1):
        f=i/steps; ph=f*pm; r=R0*(1-k*f**0.9)
        pts.append((r*math.cos(ph-math.pi/2)*1+0, R0+r*math.sin(ph-math.pi/2)))
    return pts

def comma(R0=30,turns=.95,w0=11,w1=1.6,col=BLUE,mirror=1):
    p=spiral(R0,turns); 
    return f'<path transform="scale(1,{mirror})" d="{taper(p,w0,w1,.8)}" fill="{col}"/>'

def lobe(L,W,col=BLUE):
    return f'<path d="M0,0 C{L*.25:.1f},{-W:.1f} {L*.75:.1f},{-W*.8:.1f} {L:.1f},0 C{L*.75:.1f},{W*.5:.1f} {L*.25:.1f},{W*.6:.1f} 0,0Z" fill="{col}"/>'

def fan(L,W,n=3,spread=34,col=BLUE,rot=0):
    out=f'<g transform="rotate({rot})">'
    for k in range(n):
        a=(k-(n-1)/2)*spread
        out+=f'<g transform="rotate({a})">{lobe(L*(1-.14*abs(k-(n-1)/2)),W,col)}<path d="M{L*.12:.1f},0 L{L*.7:.1f},0" stroke="#fff" stroke-width="1.1" opacity=".7"/></g>'
    return out+'</g>'

def unit(sc=1,col=BLUE):
    # lies along +x, centered at 0; outward is -y
    b=f'<g transform="translate(-4,0) rotate(0)">{comma(26,1.0,11,1.5,col,-1)}</g>'          # forward curl outward
    b+=f'<g transform="translate(4,0) scale(-1,1)">{comma(20,1.0,9,1.4,col,1)}</g>'          # backward curl inward
    b+=f'<g transform="translate(0,-3)">{fan(30,10,3,38,col,-90)}</g>'
    b+=f'<g transform="translate(0,3)">{fan(16,6,3,44,col,90)}</g>'
    b+=f'<circle r="5" fill="{col}"/><circle r="1.8" fill="#fff"/>'
    return f'<g transform="scale({sc})">{b}</g>'

NAVY_DEFS='<defs><linearGradient id="nv" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#4C6DB3"/><stop offset="1" stop-color="#22407F"/></linearGradient><linearGradient id="nv2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#3C5CA3"/><stop offset="1" stop-color="#1D3871"/></linearGradient></defs>'
def nleaf(x,y,ang,L,W,g='nv'):
    a=math.radians(ang); c,s=math.cos(a),math.sin(a)
    def P(u,v): return (x+u*c-v*s, y+u*s+v*c)
    p0=P(0,0);p1=P(L*.3,-W);p2=P(L*.75,-W*.7);p3=P(L,0);p4=P(L*.75,W*.8);p5=P(L*.3,W*1.05)
    d=f'M{p0[0]:.1f},{p0[1]:.1f} C{p1[0]:.1f},{p1[1]:.1f} {p2[0]:.1f},{p2[1]:.1f} {p3[0]:.1f},{p3[1]:.1f} C{p4[0]:.1f},{p4[1]:.1f} {p5[0]:.1f},{p5[1]:.1f} {p0[0]:.1f},{p0[1]:.1f}Z'
    m=P(L*.9,0)
    return f'<path d="{d}" fill="url(#{g})"/><path d="M{p0[0]:.1f},{p0[1]:.1f} L{m[0]:.1f},{m[1]:.1f}" stroke="#DCE6F8" stroke-opacity=".75" stroke-width="1.4" fill="none"/>'

def sprig(pts,L=44,W=13,every=2,flip=1,berries=True):
    out=stem(pts,2.6,'#2A4B8F'); n=len(pts)
    for i in range(1,n-1,every):
        x,y=pts[i]; x0,y0=pts[i-1]; x1,y1=pts[i+1]
        a=math.degrees(math.atan2(y1-y0,x1-x0))
        sc=1-.35*abs(i/n-.5)
        out+=nleaf(x,y,a-48*flip,L*sc,W*sc,'nv')+nleaf(x,y,a+48*flip,L*sc,W*sc,'nv2')
        if berries and (i//every)%3==1: out+=f'<circle cx="{x+9*math.cos(math.radians(a+90)):.1f}" cy="{y+9*math.sin(math.radians(a+90)):.1f}" r="3.2" fill="#C8A45C"/>'
    x,y=pts[-1]; a=math.degrees(math.atan2(pts[-1][1]-pts[-2][1],pts[-1][0]-pts[-2][0]))
    out+=nleaf(x,y,a,L*.8,W*.9)
    return out

def wreath():
    W,H=640,800; cx,cy=W/2,H/2; rx,ry=232,312
    b=NAVY_DEFS
    N=22
    for side in (-1,1):
        pts=[]
        for i in range(N+1):
            t=math.pi/2+side*(i/N)*math.pi*0.97
            # start bottom, travel up each side
            pts.append((cx+rx*math.cos(t), cy+ry*math.sin(t)))
        b+=sprig(pts,L=46,W=14,every=1,flip=-side)
    # lemons + bow at bottom, top accent
    b+=f'<g transform="translate({cx},{cy+ry+6}) ">{lemon(0,0,0,.55)}</g>'
    b+=f'<ellipse cx="{cx}" cy="{cy}" rx="{rx-40}" ry="{ry-40}" fill="none" stroke="#2A4B8F" stroke-width="1.2" opacity=".55"/>'
    return svg(W,H,b.replace('<defs>','<defs>',1),flt=False).replace(DEFS,DEFS+'')

def flourish():
    b=NAVY_DEFS
    pts=bez((10,10),(70,20),(130,60),(190,150),30)
    b+=sprig(pts,L=40,W=12,every=2,flip=1)
    pts2=bez((10,10),(20,70),(60,130),(150,190),30)
    b+=sprig(pts2,L=34,W=10,every=3,flip=-1)
    b+=f'<circle cx="12" cy="12" r="6" fill="#2A4B8F"/><circle cx="12" cy="12" r="2.2" fill="#fff"/>'
    return svg(250,250,b,flt=False)

def topbar():
    b=NAVY_DEFS+'<g transform="translate(110,30)">'
    for s in (-1,1):
        pts=[(s*14,0),(s*40,-5),(s*66,0),(s*92,-3)]
        b+=sprig(pts,L=22,W=6.5,every=1,flip=s,berries=False)
    b+='<path d="M0,-14 L7,0 L0,14 L-7,0Z" fill="#2A4B8F"/><circle r="2.6" fill="#fff"/></g>'
    return svg(220,60,b,flt=False)

def tile():
    # 120x120 majolica tile pattern, very light blue on white
    c='#DEE7F6'; c2='#ECF1FA'
    body=f'<rect width="120" height="120" fill="#FBFCFF"/>'
    # four-pointed star + quatrefoils
    body+=f'<path d="M60,6 L70,50 L114,60 L70,70 L60,114 L50,70 L6,60 L50,50Z" fill="none" stroke="{c}" stroke-width="2"/>'
    body+=''.join(f'<ellipse cx="60" cy="30" rx="9" ry="16" transform="rotate({k*90} 60 60)" fill="{c2}" stroke="{c}" stroke-width="1.5"/>' for k in range(4))
    body+=f'<circle cx="60" cy="60" r="7" fill="{c}"/>'
    for (x,y) in [(0,0),(120,0),(0,120),(120,120)]:
        body+=''.join(f'<ellipse cx="{x}" cy="{y-16}" rx="8" ry="14" transform="rotate({k*90} {x} {y})" fill="{c2}" stroke="{c}" stroke-width="1.4"/>' for k in range(4))
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 120 120" width="120" height="120">{body}</svg>'

for name,s in [('lemons',lemons()),('hydr',hydr()),('wreath',wreath()),('flourish',flourish()),('topbar',topbar()),('tile',tile())]:
    open(f'{OUT}/{name}.svg','w').write(s)
print('ok')
