function InitMouse() {
    const canvas = document.createElement('canvas');
    canvas.id = 'mouse-canvas';
    document.body.appendChild(canvas);
    const ctx = canvas.getContext('2d');
    const colours = ['#F73859', '#14FFEC', '#00E0FF', '#FF99FE', '#FAF15D'];

    let balls = [];
    let pressed = false;
    let longPressed = false;
    let longPress;
    let multiplier = 0;
    let width, height;
    let origin;
    let normal;

    // Make the canvas high res
    function updateSize() {
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;
        canvas.style.width = window.innerWidth + 'px';
        canvas.style.height = window.innerHeight + 'px';
        ctx.scale(1, 1);

        width = canvas.width = window.innerWidth;
        height = canvas.height = window.innerHeight;
        origin = {
            x: width,
            y: height
        };
        normal = {
            x: width,
            y: height
        };
    }

    updateSize();
    window.addEventListener('resize', updateSize, false);

    class Ball {
        constructor(x = origin.x, y = origin.y) {
            this.x = x;
            this.y = y;
            this.angle = Math.PI * 2 * Math.random();

            if (longPressed == true) {
                this.multiplier = randBetween(14 + multiplier, 15 + multiplier);
            } else {
                this.multiplier = randBetween(6, 12);
            }

            this.vx = (this.multiplier + Math.random() * 0.5) * Math.cos(this.angle);
            this.vy = (this.multiplier + Math.random() * 0.5) * Math.sin(this.angle);
            this.r = randBetween(5, 10) + 3 * Math.random();
            this.color = colours[Math.floor(Math.random() * colours.length)];
            this.direction = randBetween(-1, 1);
        }

        update() {
            this.x += this.vx - normal.x;
            this.y += this.vy - normal.y;

            normal.x = (-2 / window.innerWidth) * Math.sin(this.angle);
            normal.y = (-2 / window.innerHeight) * Math.cos(this.angle);
            // normal.y = ((-2 / window.innerHeight) * Math.cos(this.angle)) + this.direction;

            this.r -= 0.3;
            this.vx *= 0.9;
            this.vy *= 0.9;
        }
    }

    function pushBalls(count = 1, x = origin.x, y = origin.y) {
        for (let i = 0; i < count; i++) {
            balls.push(new Ball(x, y));
        }
    }

    function randBetween(min, max) {
        return Math.floor(Math.random() * max) + min;
    }

    loop();

    function loop() {
        // Alpha means "motion blur", yay!
        // ctx.fillStyle = 'rgba(20, 24, 41, 0.0)';
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        // ctx.fillRect(0, 0, canvas.width, canvas.height);

        for (let i = 0; i < balls.length; i++) {
            let b = balls[i];

            if (b.r < 0) continue;

            ctx.fillStyle = b.color;
            ctx.beginPath();
            ctx.arc(b.x, b.y, b.r, 0, Math.PI * 2, false);
            ctx.fill();

            b.update();
        }

        if (longPressed == true) {
            multiplier += multiplier <= 10 ? 0.2 : 0.0;
        } else if (!longPressed && multiplier >= 0) {
            multiplier -= 0.4;
        }

        removeBall();
        requestAnimationFrame(loop);
    }

    function removeBall() {
        for (let i = 0; i < balls.length; i++) {
            let b = balls[i];
            if (
                b.x + b.r < 0 ||
                b.x - b.r > width ||
                b.y + b.r < 0 ||
                b.y - b.r > height ||
                b.r < 0
            ) {
                balls.splice(i, 1);
            }
        }
    }

    window.addEventListener(
        'mousedown',
        function (e) {
            // if (pressed == false) clearInterval(timeOut);

            pressed = true;
            pushBalls(randBetween(10, 20), e.clientX, e.clientY);

            document.body.classList.add('is-pressed');
            longPress = setTimeout(function () {
                document.body.classList.add('is-longpress');
                longPressed = true;
            }, 750);
        },
        false
    );

    window.addEventListener(
        'mouseup',
        function (e) {
            clearInterval(longPress);
            //multiplier = 0;

            // Superblast
            if (longPressed == true) {
                document.body.classList.remove('is-longpress');
                pushBalls(
                    randBetween(50 + Math.ceil(multiplier), 100 + Math.ceil(multiplier)),
                    e.clientX,
                    e.clientY
                );
                longPressed = false;
            }

            document.body.classList.remove('is-pressed');
        },
        false
    );

    // Keep it going
    // let timeOut = setInterval(function() {
    //   pushBalls(
    //     randBetween(10, 20),
    //     origin.x + randBetween(-50, 50),
    //     origin.y + randBetween(-50, 50)
    //   );
    // }, 200);

    // Pointer stuff
    const pointer = document.createElement('span');
    pointer.classList.add('mouse-pointer');
    document.body.appendChild(pointer);

    window.addEventListener(
        'mousemove',
        function (e) {
            let x = e.clientX;
            let y = e.clientY;

            pointer.style.top = y + 'px';
            pointer.style.left = x + 'px';
        },
        false
    );

    document.addEventListener('selectionchange', function () {
        if (pressed) {
            clearInterval(longPress);
            longPressed = false;
            document.body.classList.remove('is-pressed');
            document.body.classList.remove('is-longpress');
        }
    });
}


function openNewPage(url) {
    window.open(url, "_blank");
}
function setInnerHTML(id, html) {
    var box = document.getElementById(id)
    if (box == null) {
        return;
    }
    if (box.innerHTML.length == 0) {
        box.innerHTML = html;
    }
}
function cleanInnerHTML(id) {
    var box = document.getElementById(id)
    if (box == null) {
        return;
    }
    if (box.innerHTML.length >0) {
        box.innerHTML = "";
    }

}
function deleteDiv(classname) {
    var boxs = document.getElementsByClassName(classname);
    if (boxs == null) {
        return;
    }
    for (var i = 0; i < boxs.length; i++) {
        boxs[i].remove();
    }
}
function copyUrl() {
    var clipBoardContent = this.location.href;
    window.clipboardData.setData("Text", clipBoardContent);
    alert("????????????!");
}
function tool_scroll(t, i) {

    if (i === "dispose") {

        window.onscroll = null;
        return
    }
    var r = function () {
        var t2 = document.body.scrollHeight;
        var t1 = document.documentElement.scrollTop || document.body.scrollTop;
        var r = t2 - t1;
        t.invokeMethodAsync(i, r)
    };
    r();
    window.onscroll = function () { r() };
}
function getScrollBottom() {
    this.docScrollTop = document.documentElement && document.documentElement.scrollTop
    // console.log(this.docScrollTop, '??????')    
    this.docClientHeight = document.body.clientHeight && document.documentElement.clientHeight
    // console.log(this.docClientHeight, '????????????')
    this.docScrollHeight = document.body.scrollHeight
    // console.log(this.docScrollHeight, '??????????????????')
    const aaa = this.docScrollHeight - this.docScrollTop - this.docClientHeight

    return aaa;
}


function initDebugTool() {
    // init vConsole
    eruda.init();
   // var vConsole = new VConsole();
    console.log('????????????????????????');
}
function initMouseJs() {
    var script2 = document.createElement('script');
    script2.setAttribute('type', 'text/javascript');
    script2.setAttribute('src', 'js/mouse.js');
    document.body.appendChild(script2);
}
function highlightAllCode() {
    hljs.highlightAll();
}

var editor;
function initEditorMd(markdownstring) {
    $(function () {
        editor = editormd("editor", {
            width: "100%",
            height: "100%",
           // tocm: true, // Using [TOCM]
           // tex: true, // ??????????????????TeX???????????????????????????
           // flowChart: true, // ????????????????????????????????????
            placeholder: "?????????Markdown??????????????????????????????????????????????????????",
            markdown: markdownstring,     // dynamic set Markdown text
            path: "_content/SDAllianceWebSite.Shared/editor.md/lib/"  // Autoload modules mode, codemirror, marked... dependents libs path
        });
    });
}

function getEditorMdContext() {
    return editor.getMarkdown();
}
function addEditorMdContext(markdownstring) {
    editor.appendMarkdown(markdownstring);
}

function initUploadButton(objRef, up_to_chevereto, up_img_label, tmpSecretId, tmpSecretKey, securityToken, startTime, expiredTime) {
    jQuery(up_to_chevereto).change(function () {
        for (var i = 0; i < this.files.length; i++) {

            var f = this.files[i];

            var filename = f.name;
            var index = filename.lastIndexOf(".");
            var suffix = filename.substring(index + 1);


            jQuery(up_img_label).html('<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> ?????????...');

            uploadFile(f, suffix, tmpSecretId, tmpSecretKey, securityToken, startTime, expiredTime,
                function (res) {
                    objRef.invokeMethodAsync('UpLoaded', res + '||' + f.size);
                    //   document.getElementsByClassName("tui-editor-contents")[0].innerHTML='<a href="' + res.image.url + '"><img src="' + res.image.url + '" alt="' + res.image.title + '"></img></a>';
                    jQuery(up_img_label).html('<i class="fa fa-check" aria-hidden="true"></i> ????????????,????????????');
                },
                function () {
                    jQuery(up_img_label).html('<i class="fa fa-times" aria-hidden="true"></i> ???????????????????????????');
                });


        }
    });
}

function uploadFile(file, suffix, tmpSecretId, tmpSecretKey, securityToken, startTime, expiredTime, callback, error) {
    // ???????????????
    var cos = new COS({
        getAuthorization: function (options, callback) {
            callback({
                TmpSecretId: tmpSecretId,
                TmpSecretKey: tmpSecretKey,
                SecurityToken: securityToken,
                // ????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????
                StartTime: startTime, // ??????????????????????????????1580000000
                ExpiredTime: expiredTime, // ??????????????????????????????1580000900
            });
        }
    });
    // ????????????????????? cos ???????????? COS ?????????
    // TODO

    var myDate = new Date();

    var forder = myDate.getFullYear() + '-' + (myDate.getMonth() + 1).toString() + '-' + myDate.getDate() + '/';

    cos.putObject({
        Bucket: 'image-1256103450', /* ?????? */
        Region: 'ap-guangzhou',     /* ???????????????????????????????????? */
        Key: forder,              /* ?????? */
        Body: '',
    }, function (err, data) {
        //error();
    });

    var fileName = forder + 'A-' + randomNum(100000, 9999999) + randomNum(100000, 9999999) + '.' + suffix;

    cos.putObject({
        Bucket: 'image-1256103450', /* ?????? */
        Region: 'ap-guangzhou',     /* ???????????????????????????????????? */
        Key: fileName,              /* ?????? */
        StorageClass: 'STANDARD',
        Body: file, // ??????????????????
        onProgress: function (progressData) {
            console.log(JSON.stringify(progressData));
        }
    }, function (err, data) {
        if (err != null) {
            error();
        }
        else {
            callback('//' + data.Location);
        } 
    });

}

function randomNum(minNum, maxNum) {
    switch (arguments.length) {
        case 1:
            return parseInt(Math.random() * minNum + 1, 10);
            break;
        case 2:
            return parseInt(Math.random() * (maxNum - minNum + 1) + minNum, 10);
            break;
        default:
            return 0;
            break;
    }
}


function addfavorite() {
    var ctrl = (navigator.userAgent.toLowerCase()).indexOf('mac') != -1 ? 'Command/Cmd' : 'CTRL';
    try {
        if (document.all) { //IE????????????
            try {
                window.external.toString(); //360??????????????????window.external???????????????
                window.alert("???????????????360??????????????????????????????????????????\n??????????????????????????????????????? ???????????? ctrl+D ?????????");
            }
            catch (e) {
                try {
                    window.external.addFavorite(window.location, document.title);
                }
                catch (e) {
                    window.external.addToFavoritesBar(window.location, document.title);  //IE8
                }
            }
        }
        else if (window.sidebar) { //firfox????????????
            window.sidebar.addPanel(document.title, window.location, "");
        }
        else {
            alert('??????????????????????????????' + ctrl + ' + D ??????????????????~');
        }
    }
    catch (e) {
        window.alert("??????IE???????????????bug????????????????????????\n????????????????????????????????????\n HKEY_CLASSES_ROOT\\TypeLib\\{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}\\1.1\\0\\win32 \n??? C:\\WINDOWS\\system32\\shdocvw.dll ?????? C:\\WINDOWS\\system32\\ieframe.dll ");
    }
}