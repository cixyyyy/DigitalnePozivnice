import math, random, sys
sys.argv=[sys.argv[0]]
from itsvg import *
R2=random.Random(11)

def peony(x,y,r,inner='#FFF3EC',outer='#F6C9C4',rings=4):
    g=f'<g transform="translate({x:.1f},{y:.1f}) rotate({R2.uniform(0,360):.0f})">'
    for k in range(rings):
        n=7+k*2; rr=r*(1-k*.2); pr=rr*.62
        col=outer if k<rings-2 else inner
        for j in range(n):
            a=j*360/n+k*13
            g+=f'<ellipse cx="0" cy="{-rr*.55:.1f}" rx="{pr*.62:.1f}" ry="{pr:.1f}" transform="rotate({a:.1f})" fill="{col}" stroke="#E4A9A6" stroke-opacity=".5" stroke-width=".8"/>'
        if k==0: g+=f'<circle r="{rr*.78:.1f}" fill="{outer}" opacity=".35"/>'
    g+=f'<circle r="{r*.1:.1f}" fill="#E9B65A"/></g>'
    return g

def anemone(x,y,r,rot=0):
    g=f'<g transform="translate({x:.1f},{y:.1f}) rotate({rot})">'
    for j in range(7):
        g+=f'<ellipse cx="0" cy="{-r*.5:.1f}" rx="{r*.42:.1f}" ry="{r*.62:.1f}" transform="rotate({j*360/7:.1f})" fill="#FFFFFF" stroke="#CFC3DC" stroke-width="1"/>'
    g+=f'<circle r="{r*.3:.1f}" fill="#2A3558"/><circle r="{r*.36:.1f}" fill="none" stroke="#6B5B8C" stroke-width="1.4"/>'
    g+=''.join(f'<circle cx="{r*.22*math.cos(a):.1f}" cy="{r*.22*math.sin(a):.1f}" r="1.2" fill="#8E86B8"/>' for a in [i*.9 for i in range(7)])
    return g+'</g>'

def primula(x,y,r):
    g=f'<g transform="translate({x:.1f},{y:.1f}) rotate({R2.uniform(0,72):.0f})">'
    for j in range(5):
        g+=f'<ellipse cx="0" cy="{-r*.5:.1f}" rx="{r*.46:.1f}" ry="{r*.58:.1f}" transform="rotate({j*72})" fill="#F8E39A" stroke="#E2C25B" stroke-width="1"/>'
    return g+f'<circle r="{r*.22:.1f}" fill="#E0A83A"/></g>'

def eucal(x,y,ang,L):
    # round grey-green leaves along a stem
    a=math.radians(ang); out=''
    ex,ey=x+L*math.cos(a),y+L*math.sin(a)
    out+=f'<path d="M{x:.0f},{y:.0f} Q{(x+ex)/2+8:.0f},{(y+ey)/2-8:.0f} {ex:.0f},{ey:.0f}" stroke="#7C8F76" stroke-width="2.4" fill="none"/>'
    for i in range(1,7):
        t=i/7; px=x+(ex-x)*t; py=y+(ey-y)*t
        for s in (-1,1):
            out+=f'<ellipse cx="{px+s*9*math.cos(a+1.57):.1f}" cy="{py+s*9*math.sin(a+1.57):.1f}" rx="{L*.08:.1f}" ry="{L*.065:.1f}" fill="#9DB19A" stroke="#6F8570" stroke-opacity=".5" transform="rotate({ang+s*30} {px:.0f} {py:.0f})"/>'
    return out

def bouquet(flip=False,W=520,H=860):
    b=''
    # background greenery
    for (x,y,a,L) in [(60,560,-70,330),(120,600,-60,300),(200,540,-80,330),(30,430,-85,260),(250,640,-55,260),(150,420,-75,280)]:
        b+=eucal(x,y,a,L)
    for (x,y,a,L,W2,g) in [(80,700,-45,150,34,'lf2'),(190,740,-30,150,36,'lf'),(300,780,-15,150,34,'lf2'),(40,620,-70,130,30,'lf')]:
        b+=leaf(x,y,a,L,W2,g)
    # flowers: big to small
    b+=hydrangea(120,420,95,110,['#B8C8EC','#9DB2E2','#7F99D3','#6682C2'],n=70,fr=21)
    b+=hydrangea(300,300,70,80,['#FFFFFF','#F3F0E6','#E3EDF5'],n=34,fr=19)
    b+=peony(210,560,92,'#FFF6EE','#F8D5CC')
    b+=peony(80,640,70,'#FFFFFF','#F0D9DF')
    b+=anemone(330,500,52,10)+anemone(60,520,44,-20)+anemone(250,380,36,30)
    b+=primula(150,720,34)+primula(330,660,30)+primula(40,430,26)+primula(240,300,24)
    b+=lemon(300,790,-10,.75)
    b+=peony(330,720,58,'#FFFFFF','#F6C9C4')
    b+=''.join(floret(R2.uniform(20,360),R2.uniform(740,840),R2.uniform(10,15),R2.choice(['#9DB2E2','#F3F0E6','#F8E39A','#FFFFFF']),R2.uniform(0,90)) for _ in range(30))
    body=f'<g transform="scale({-1 if flip else 1},1) translate({-W if flip else 0},0)">{b}</g>'
    return body

def chateau(ox=0,oy=0,sc=1.0):
    st='#E8DBBE'
    g=f'<g transform="translate({ox},{oy}) scale({sc})">'
    g+='''<defs><linearGradient id="stone" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#F0E4C9"/><stop offset="1" stop-color="#D7C49C"/></linearGradient>
<linearGradient id="roof" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#9AA3BB"/><stop offset="1" stop-color="#5D6683"/></linearGradient>
<linearGradient id="roof2" x1="1" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#8F98B2"/><stop offset="1" stop-color="#4F5876"/></linearGradient>
<linearGradient id="win" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#6B7FA3"/><stop offset="1" stop-color="#2C3A57"/></linearGradient></defs>'''
    # cypress
    for cx in (226,774,206,794):
        h=150 if cx in (226,774) else 120
        g+=f'<path d="M{cx},{720-h*1.9} C{cx+22},{720-h*1.3} {cx+24},{720-h*.5} {cx},{720} C{cx-24},{720-h*.5} {cx-22},{720-h*1.3} {cx},{720-h*1.9}Z" fill="#4C6A47"/><path d="M{cx-4},{720-h*1.7} C{cx+8},{720-h} {cx+8},{720-h*.4} {cx-2},{720-6}" stroke="#7E9A6E" stroke-width="5" fill="none" opacity=".5"/>'
    # wings & center
    g+=f'<rect x="352" y="430" width="296" height="292" fill="url(#stone)"/>'
    # roofs
    g+='<path d="M338,436 L372,352 L628,352 L662,436Z" fill="url(#roof)"/><path d="M372,352 L628,352 L618,340 L382,340Z" fill="#6D7694"/>'
    g+=''.join(f'<path d="M{x},{352} q{R2.uniform(-3,3):.0f},{R2.uniform(30,70):.0f} {R2.uniform(-4,4):.0f},{R2.uniform(60,84):.0f}" stroke="#4A5370" stroke-opacity=".35" stroke-width="{R2.uniform(2,5):.1f}" fill="none"/>' for x in range(380,626,22))
    for cx_ in (402,432,568,598): g+=f'<rect x="{cx_-7}" y="318" width="14" height="30" fill="#D8C7A2"/><rect x="{cx_-9}" y="314" width="18" height="7" fill="#C9B68E"/>'
    # towers
    for tx in (255,635):
        g+=f'<rect x="{tx}" y="420" width="110" height="302" fill="url(#stone)"/>'
        g+=f'<ellipse cx="{tx+55}" cy="420" rx="62" ry="10" fill="#D2BF98"/>'
        g+=f'<path d="M{tx-10},420 L{tx+55},196 L{tx+120},420Z" fill="url(#{"roof" if tx<400 else "roof2"})"/>'
        g+=f'<path d="M{tx+55},196 L{tx+55},160" stroke="#B9965A" stroke-width="3"/><circle cx="{tx+55}" cy="158" r="4" fill="#C9A85E"/>'
        g+=''.join(f'<path d="M{tx+55+d},{240+abs(d)*.2:.0f} L{tx+55+d*1.45:.0f},418" stroke="#434C69" stroke-opacity=".28" stroke-width="{R2.uniform(2,5):.1f}"/>' for d in range(-40,45,12))
        g+=f'<path d="M{tx+36},330 q19,-30 38,0 v18 h-38z" fill="#D8C7A2"/><rect x="{tx+44}" y="320" width="22" height="26" fill="url(#win)"/>'
    # windows
    def win(x,y,w=26,h=56,shut=True):
        o=f'<rect x="{x-5}" y="{y-5}" width="{w+10}" height="{h+10}" rx="2" fill="#F4EAD3" stroke="#C6B38D" stroke-width="1"/>'
        o+=f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="url(#win)"/>'
        o+=f'<path d="M{x+w/2},{y} V{y+h} M{x},{y+h/3:.0f} H{x+w} M{x},{y+h*2/3:.0f} H{x+w}" stroke="#F1EBDD" stroke-width="2"/>'
        if shut: o+=f'<rect x="{x-17}" y="{y-3}" width="11" height="{h+6}" fill="#B8C5B1" stroke="#98A793" stroke-width=".8"/><rect x="{x+w+6}" y="{y-3}" width="11" height="{h+6}" fill="#B8C5B1" stroke="#98A793" stroke-width=".8"/>'
        return o
    for x in (400,452,522,574):
        g+=win(x,452)+win(x,556)
    for tx in (255,635):
        g+=win(tx+42,470,26,56,False)+win(tx+42,570,26,56,False)+win(tx+42,650,26,48,False)
    # dormers
    for dx in (410,500,590):
        g+=f'<path d="M{dx-20},388 L{dx},362 L{dx+20},388Z" fill="#6D7694"/><rect x="{dx-14}" y="386" width="28" height="30" fill="#EADFC5"/><rect x="{dx-9}" y="390" width="18" height="24" fill="url(#win)"/><path d="M{dx},390 V414" stroke="#F1EBDD" stroke-width="1.6"/>'
    # balcony & door
    g+='<rect x="476" y="528" width="48" height="8" fill="#D9C8A3"/>'+''.join(f'<rect x="{476+i*5}" y="518" width="2.4" height="10" fill="#EFE5CF"/>' for i in range(10))
    g+='<path d="M468,722 V656 q32,-46 64,0 V722Z" fill="#EADFC5" stroke="#C6B38D"/><path d="M476,722 V660 q24,-34 48,0 V722Z" fill="#2F6068"/><path d="M500,626 V722" stroke="#9FC0BE" stroke-width="1.6"/>'
    # steps
    for i in range(4): g+=f'<rect x="{462-i*6}" y="{722+i*6}" width="{76+i*12}" height="7" fill="{["#E5D8BA","#DCCDAB","#D3C29C","#CAB78F"][i]}"/>'
    # balustrade
    for (x0,x1) in ((355,455),(545,645)):
        g+=f'<rect x="{x0}" y="706" width="{x1-x0}" height="6" fill="#E6DAC0"/><rect x="{x0}" y="728" width="{x1-x0}" height="6" fill="#D4C4A0"/>'+''.join(f'<rect x="{x}" y="712" width="4" height="16" fill="#EFE5CF"/>' for x in range(x0+2,x1-3,9))
    # climbing greenery
    for (x,y) in [(380,640),(420,600),(540,600),(620,640),(300,560),(700,560),(300,640),(700,640),(360,540),(640,540),(450,680),(550,680)]:
        for k in range(6):
            g+=f'<ellipse cx="{x+R2.uniform(-22,22):.0f}" cy="{y+R2.uniform(-40,40):.0f}" rx="8" ry="4.6" fill="{R2.choice(["#6E8E5A","#4F7447","#86A56B"])}" transform="rotate({R2.uniform(0,180):.0f} {x} {y})"/>'
        g+=f'<circle cx="{x+R2.uniform(-18,18):.0f}" cy="{y+R2.uniform(-30,30):.0f}" r="4" fill="#FFFFFF" stroke="#E4DCC8"/>'
    return g+'</g>'

def sky_defs():
    return '<defs><linearGradient id="sky" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#7FB2E5"/><stop offset=".55" stop-color="#BBD8F1"/><stop offset="1" stop-color="#EAF3FB" stop-opacity="0"/></linearGradient><filter id="cl"><feGaussianBlur stdDeviation="14"/></filter><radialGradient id="fade" cx=".5" cy=".45" r=".62"><stop offset=".62" stop-color="#fff"/><stop offset="1" stop-color="#000"/></radialGradient><mask id="vm"><rect width="1000" height="900" fill="url(#fade)"/></mask></defs>'

def scene(sky=True):
    W,H=1000,900
    b=''
    if sky:
        b+='<g mask="url(#vm)"><rect width="1000" height="780" fill="url(#sky)"/>'
        for (x,y,rx,ry,o) in [(220,120,150,34,.85),(330,160,120,26,.7),(760,90,170,38,.9),(640,200,130,28,.7),(500,60,110,22,.6),(880,210,100,24,.7)]:
            b+=f'<ellipse cx="{x}" cy="{y}" rx="{rx}" ry="{ry}" fill="#fff" opacity="{o}" filter="url(#cl)"/>'
        b+='</g>'
    b+=f'<g filter="url(#paint)">{chateau(-60,-70,1.12)}</g>'
    # lawn/hedge base
    b+=''.join(f'<ellipse cx="{x}" cy="{R2.randint(770,800)}" rx="{R2.randint(26,44)}" ry="{R2.randint(14,22)}" fill="{R2.choice(["#6E8E5A","#5B7F4F","#7FA066"])}"/>' for x in range(300,720,34))
    b+=f'<g filter="url(#paint)"><g transform="translate(-150,70) scale(.92)">{bouquet(False)}</g><g transform="translate(670,70) scale(.92)">{bouquet(True)}</g></g>'
    # bottom garden florets
    b+=''.join(floret(R2.uniform(280,720),R2.uniform(800,885),R2.uniform(12,18),R2.choice(['#9DB2E2','#F3F0E6','#F8E39A','#FFFFFF','#B8C8EC']),R2.uniform(0,90)) for _ in range(46))
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {W} {H}" width="{W}" height="{H}">{DEFS}{sky_defs()}{b}</svg>'


def cluster(w=400,h=320,flip=False,bot=False):
    b=''
    for (x,y,a,L,W2,g) in [(30,260,-30,150,32,'lf2'),(110,290,-10,150,34,'lf'),(20,170,-60,130,30,'lf'),(200,300,-5,120,28,'lf2'),(40,80,-80,110,28,'lf2')]:
        b+=leaf(x,y,a,L,W2,g)
    b+=hydrangea(120,150,85,95,['#B8C8EC','#9DB2E2','#7F99D3','#6682C2'],n=52,fr=20)
    b+=hydrangea(250,95,60,55,['#FFFFFF','#F3F0E6','#E3EDF5'],n=22,fr=18)
    b+=peony(90,250,56,'#FFF6EE','#F8D5CC')+anemone(215,215,38,10)+anemone(60,60,30,20)
    b+=primula(170,285,22)+primula(250,170,20)+primula(30,150,18)+lemon(300,260,-8,.55)
    b+=''.join(floret(R2.uniform(10,330),R2.uniform(20,300),R2.uniform(9,13),R2.choice(['#9DB2E2','#F3F0E6','#F8E39A','#FFFFFF']),R2.uniform(0,90)) for _ in range(14))
    t=f'translate({w if flip else 0},{h if bot else 0}) scale({-1 if flip else 1},{-1 if bot else 1})'
    return f'<g transform="{t}">{b}</g>'

def bow(cx,cy):
    n='#2E4A8E'
    g=f'<g transform="translate({cx},{cy})">'
    g+=f'<path d="M0,0 C-70,-60 -150,-30 -120,20 C-100,50 -40,30 0,0Z" fill="{n}"/><path d="M0,0 C70,-60 150,-30 120,20 C100,50 40,30 0,0Z" fill="{n}"/>'
    g+=f'<path d="M-6,-6 C-60,-34 -108,-16 -98,10" stroke="#7F99D3" stroke-width="3" fill="none" opacity=".7"/><path d="M6,-6 C60,-34 108,-16 98,10" stroke="#7F99D3" stroke-width="3" fill="none" opacity=".7"/>'
    g+=f'<path d="M-6,8 C-60,60 -130,90 -170,150 C-140,150 -110,130 -90,120 C-60,100 -20,60 8,16Z" fill="#3A5AA3"/><path d="M6,8 C60,60 130,90 170,150 C140,150 110,130 90,120 C60,100 20,60 -8,16Z" fill="#3A5AA3"/>'
    g+=f'<rect x="-16" y="-14" width="32" height="30" rx="8" fill="#24407F"/><circle cy="1" r="4" fill="#C8A45C"/></g>'
    return g

def svgw(w,h,body):
    return f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w} {h}" width="{w}" height="{h}">{DEFS}<g filter="url(#paint)">{body}</g></svg>'
top=cluster(flip=False)+f'<g transform="translate(800,0) scale(-1,1)">{cluster()}</g>'
# top sprig between clusters + bow
sp=stem(bez((330,40),(380,60),(420,60),(470,40),12),3,'#6E8E5A')
for xx in range(340,470,26): sp+=leaf(xx,48,-20 if xx<400 else 200,34,9,'lf')
top+=sp+bow(400,70)
open(OUT+'/frame_top.svg','w').write(svgw(800,320,top))
open(OUT+'/frame_bot.svg','w').write(svgw(800,320,cluster(bot=True)+f'<g transform="translate(800,0) scale(-1,1)">{cluster(bot=True)}</g>'))
side=''
side+=stem(bez((40,0),(10,70),(70,130),(40,200),16),3,'#6E8E5A')
for (x,y,a) in [(36,30,30),(24,70,-150),(48,112,20),(40,160,-160)]: side+=leaf(x,y,a,34,9,'lf')
for (x,y) in [(50,92),(30,140),(46,40)]: side+=floret(x,y,9,R2.choice(['#9DB2E2','#FFFFFF','#F8E39A']),20)
open(OUT+'/frame_side.svg','w').write(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 200" width="80" height="200">{DEFS}{side}</svg>')
open(OUT+'/bouq.svg','w').write(f'<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 520 860" width="520" height="860">{DEFS}<g filter="url(#paint)">{bouquet(False)}</g></svg>')
open(OUT+'/scene_sky.svg','w').write(scene(True))
open(OUT+'/scene_flat.svg','w').write(scene(False))
print('ok')
