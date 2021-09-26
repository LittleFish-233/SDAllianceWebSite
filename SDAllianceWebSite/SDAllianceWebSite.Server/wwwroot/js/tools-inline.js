
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
    alert("复制成功!");
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
    // console.log(this.docScrollTop, '高度')    
    this.docClientHeight = document.body.clientHeight && document.documentElement.clientHeight
    // console.log(this.docClientHeight, '页面高度')
    this.docScrollHeight = document.body.scrollHeight
    // console.log(this.docScrollHeight, '文档实际高度')
    const aaa = this.docScrollHeight - this.docScrollTop - this.docClientHeight

    return aaa;
}


function initDebugTool() {
    // init vConsole
    eruda.init();
   // var vConsole = new VConsole();
    console.log('成功启动调试工具');
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
           // tex: true, // 开启科学公式TeX语言支持，默认关闭
           // flowChart: true, // 开启流程图支持，默认关闭
            placeholder: "这里是Markdown编辑器，可以点击右侧问号了解语法详情",
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


            jQuery(up_img_label).html('<i class="fa fa-spinner fa-spin" aria-hidden="true"></i> 上传中...');

            uploadFile(f, suffix, tmpSecretId, tmpSecretKey, securityToken, startTime, expiredTime,
                function (res) {
                    objRef.invokeMethodAsync('UpLoaded', res + '||' + f.size);
                    //   document.getElementsByClassName("tui-editor-contents")[0].innerHTML='<a href="' + res.image.url + '"><img src="' + res.image.url + '" alt="' + res.image.title + '"></img></a>';
                    jQuery(up_img_label).html('<i class="fa fa-check" aria-hidden="true"></i> 上传成功,继续上传');
                },
                function () {
                    jQuery(up_img_label).html('<i class="fa fa-times" aria-hidden="true"></i> 上传失败，重新上传');
                });


        }
    });
}

function uploadFile(file, suffix, tmpSecretId, tmpSecretKey, securityToken, startTime, expiredTime, callback, error) {
    // 初始化实例
    var cos = new COS({
        getAuthorization: function (options, callback) {
            callback({
                TmpSecretId: tmpSecretId,
                TmpSecretKey: tmpSecretKey,
                SecurityToken: securityToken,
                // 建议返回服务器时间作为签名的开始时间，避免用户浏览器本地时间偏差过大导致签名错误
                StartTime: startTime, // 时间戳，单位秒，如：1580000000
                ExpiredTime: expiredTime, // 时间戳，单位秒，如：1580000900
            });
        }
    });
    // 接下来可以通过 cos 实例调用 COS 请求。
    // TODO

    var myDate = new Date();

    var forder = myDate.getFullYear() + '-' + (myDate.getMonth() + 1).toString() + '-' + myDate.getDate() + '/';

    cos.putObject({
        Bucket: 'image-1256103450', /* 必须 */
        Region: 'ap-guangzhou',     /* 存储桶所在地域，必须字段 */
        Key: forder,              /* 必须 */
        Body: '',
    }, function (err, data) {
        //error();
    });

    var fileName = forder + 'A-' + randomNum(100000, 9999999) + randomNum(100000, 9999999) + '.' + suffix;

    cos.putObject({
        Bucket: 'image-1256103450', /* 必须 */
        Region: 'ap-guangzhou',     /* 存储桶所在地域，必须字段 */
        Key: fileName,              /* 必须 */
        StorageClass: 'STANDARD',
        Body: file, // 上传文件对象
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
        if (document.all) { //IE类浏览器
            try {
                window.external.toString(); //360浏览器不支持window.external，无法收藏
                window.alert("国内开发的360浏览器等不支持主动加入收藏。\n您可以尝试通过浏览器菜单栏 或快捷键 ctrl+D 试试。");
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
        else if (window.sidebar) { //firfox等浏览器
            window.sidebar.addPanel(document.title, window.location, "");
        }
        else {
            alert('您可以尝试通过快捷键' + ctrl + ' + D 加入到收藏夹~');
        }
    }
    catch (e) {
        window.alert("因为IE浏览器存在bug，添加收藏失败！\n解决办法：在注册表中查找\n HKEY_CLASSES_ROOT\\TypeLib\\{EAB22AC0-30C1-11CF-A7EB-0000C05BAE0B}\\1.1\\0\\win32 \n将 C:\\WINDOWS\\system32\\shdocvw.dll 改为 C:\\WINDOWS\\system32\\ieframe.dll ");
    }
}
