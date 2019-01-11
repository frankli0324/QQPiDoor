# WTF is this thing

这是一个运行在树莓派上的 .NET Core 程序，能够控制某个门的开关。

## 0x0 Contents

1. 初成型
2. 加路障
3. 关于树莓派的GPIO

## 0x1 初成型

    这东西是咋来的？我把开关。。。。压断了。。。。。  
    怪我咯，这开关压不开)小声BB  

    刚好手边有个暂时没什么用的树莓派，那就试着看能不能用树莓派开门呗  

首先看到两根线，看起来两根线碰在一起门就能开。。但是由于不能确定两根线间的电压，所以大概是用一个继电器  
然后@pupile师傅带来了一个。。。动作电压是12V的  
很不幸，树莓派只能供5V的电，@nenmao 师傅表示要用三极管升电压，还得有一个独立的12V电源，过于复杂，pass)

这天半夜，我突发奇想，抱着树莓派坏了就坏了吧的心态将门的两根线直接怼到了树莓派上...正常工作！  
好啊.jpg

于是便有了[commit 78636de](https://github.com/frankli0324/QQPiDoor/blob/78636def4b90d4756100668bfed0f53ee8cd83ea/Program.cs)

## 0x2 加路障

由上方代码可以看出，任何人只要加了bot的好友，就能随心所欲地开门。不能排除有同学/师傅捣乱（逃  
于是 @nenmao 师傅提出应当添加鉴权步骤。  
于是有了[commit 1ffd495](https://github.com/frankli0324/QQPiDoor/blob/1ffd495bd21e437bb7615357fa7c767874c9ddf6/Program.cs#L88)， 经过重构后得到[commit c38d97c](https://github.com/frankli0324/QQPiDoor/blob/c38d97ca41feeb4b6f33bdabde4779a78beefe8b/MacChecker.cs)  

```flowchart
st=>start: Start
op1=>operation: QQ bot收到消息
isRegister=>condition: 开头不是
'register'
register=>parallel: 进行MAC与QQ的绑定
isOnline=>condition: 设备是否
连接到了
Wi-Fi
timecheck=>condition: 距设备上次断开
是否小于10min
block=>parallel: 阻断
e=>end: done

st->op1->isRegister
isRegister(no)->register
op2=>operation: door open
isRegister(yes)->isOnline
isOnline(yes)->op2
isOnline(no)->timecheck
timecheck(yes)->op2
timecheck(no)->block
op2->e
```

## 0x3 关于树莓派的GPIO

之前一直以为树莓派的GPIO只能通过各种库(e.g. C++的WiringPi)调用，然而实际上linux提供了GPIO的操作接口`/sys/class/gpio`。  
简单的使用方式通过shell脚本的形式展现如下

```sh
cd /sys/class/gpio
ls
# export, unexport, gpio0, gpio128
echo "26" > /sys/class/gpio/export
# 启用GPIO26
ls
# export, unexport, gpio0, <gpio26>, gpio128
echo "out" > /sys/class/gpio/gpio26/direction
# in与out的意思比较明显了
echo "1" > /sys/class/gpio/gpio26/value
# 输出高电平，若direction为in则可以从value中取值
echo "26" > /sys/class/gpio/unexport
# 禁用GPIO26
ls
# export, unexport, gpio0, gpio128
```

也就是说，任何能操作文件的能运行在树莓派上的语言都能操作GPIO。
