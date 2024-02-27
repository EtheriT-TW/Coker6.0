"use strict";
const setBGCanvas = function () {
    const offset = {
        "1500": [1,1.1,1.8],
        "1200": [0.8, 1.1, 2.2],
        "768": [0.9, 1.1, 2],
        "else": [1, 1.1, 1.5]
    };
    let setting = null;
    $.fn.extend({
        BGCanvas: function () {
            const $self = $(this);
            const run = function () {
                if (!isNaN($self.data("type"))) {
                    switch (parseInt($self.data("type"))) {
                        case 1:
                            $self.BGCanvasType1();
                            break;
                    }
                }
            }
            if ($self.height() == 0) {
                $self.find("img").on("load", function () {
                    run();
                });
            } else run();
            return $self;
        },
        BGCanvasType1: function () {
            let $self = $(this);
            let $c = $(this);
            if ($self.length != 0 && $self.get(0).tagName.toLowerCase() != "canvas") {
                if ($self.find("canvas").length == 0) {
                    $c = $("<canvas>");
                    $self.append($c);
                }
            }
            if ($self.length == 0) return $self;
            var W, H, L, n = 18, c = 45, dc = 0.25;
            const { sin, cos, PI, sqrt, random, floor, ceil, round, abs } = Math;

            function initSet() {
                var cnv = $c.get(0);
                var ctx = cnv.getContext("2d");
                function init() {
                    if (window.innerWidth > 1500) setting = offset["1500"];
                    else if (window.innerWidth > 1200) setting = offset["1200"];
                    else if (window.innerWidth > 768) setting = offset["768"];
                    else setting = offset["else"];
                    W = $self.width() * setting[0];
                    H = $self.height() * setting[0];
                    console.log(setting);
                    cnv.width = W;
                    cnv.height = H;
                    L = (W < H ? W : H) / 2;
                    ctx.fillStyle = "rgba(245,251,251,0)";
                    ctx.fillRect(0, 0, W, H);
                }
                init();
                window.onresize = init;

                function Point() {
                    this.ang = 2 * PI * random();
                    this.dang = (-0.5 + random()) / 40;
                    this.r = 2 * L / setting[2];
                    this.x = W / 2 + this.r * cos(this.ang);
                    this.y = H / 2 + setting[1] * this.r * sin(this.ang);
                    this.update = function () {
                        this.ang += this.dang;
                        this.x = W / 2 + this.r * cos(this.ang);
                        this.y = H / 2 + setting[1] * this.r * sin(this.ang);
                    }
                }

                var ctrls = [];
                for (let i = 0; i < n; i++) {
                    ctrls.push(new Point());
                }

                function animate() {
                    ctx.fillStyle = "rgba(255,255,255,0.03)";
                    ctx.fillRect(0, 0, W, H);
                    ctx.beginPath();
                    ctx.moveTo((ctrls[0].x + ctrls[n - 1].x) / 2, (ctrls[0].y + ctrls[n - 1].y) / 2);
                    for (let p = 0; p < n; p++) {
                        let q = p + 1;
                        if (q == n) q = 0;
                        ctx.quadraticCurveTo(ctrls[p].x, ctrls[p].y, (ctrls[p].x + ctrls[q].x) / 2, (ctrls[p].y + ctrls[q].y) / 2);
                        ctrls[p].update();
                    }
                    ctx.strokeStyle = `hsl(${round(180 + c)}deg, 100%, 70%)`;
                    //ctx.shadowBlur = L * 25 / 432;
                    //ctx.shadowColor = `hsl(${round(180 + c)}deg, 100%, 50%)`;
                    ctx.lineWidth = L * 2 / 432;

                    ctx.stroke();
                    ctx.shadowColor = "transparent";
                    c += dc;
                    if (c >= 100 || c <= 45) dc = -dc;
                    if (c == 45) {
                        ctx.fillStyle = "rgba(0,0,0,0)";
                        ctx.fillRect(0, 0, W, H);
                    }
                    window.requestAnimationFrame(animate);
                }
                animate();
            }
            initSet();
            return $self;
        }
    });
    //if (!co.isMobileDevice()) $(".BGCanvas").BGCanvas();
    $(".BGCanvas").BGCanvas();
}