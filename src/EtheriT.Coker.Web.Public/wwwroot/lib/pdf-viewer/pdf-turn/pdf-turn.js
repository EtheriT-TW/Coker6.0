/********************************************************/
/**     HERE MAIN MODIFIED PART FOR turnjs SUPPORT     **/
/********************************************************/
/// requires jquery and turnjs
/// all code added in viewer.js (from pdfjs build) in order to support 
/// flipbook is commented with '$FB:' string to allow to find it easilly 

var bookFlip = {
	_width: [],		//flipbook pages width
	_height: [],	//flipbook pages height
	active: false,	//flipbook mode on
	_spreadBk: NaN,	//spread mode backup to restore
	_evSpread: null,//spread mode changed default event handler 
	_spread: NaN,	//spread page mode
	toStart: false,	//PDFjs require flipbook at start
	_intoView: null,//link handler default function
	_visPages: null,//visible pages function
	_ready: false,	//ready to start flipbook
	scrollMode: 4,
	lName: "_listeners",

	// event listeners when bookFlip need different handling 
	init: function (parm) {
		this.scrollMode = parm.mode;
		this.lName = parm.lName;
		console.log(this.scrollMode);
		const eventBus = PDFViewerApplication.eventBus;
		eventBus.on('rotationchanging', () => { console.log("rotationchanging"); this.rotate() });
		eventBus.on('scalechanging', () => { console.log("scalechanging"); this.resize() });
		eventBus.on('pagechanging', () => { console.log("pagechanging"); this.flip() });

		eventBus.on('documentinit', () => {
			console.log("documentinit");
			this.stop();
			PDFViewerApplication.pdfViewer.currentPageScaleValue = 'page-fit'; // 設定縮放為頁面大小
			this._ready = false;
		});

		eventBus.on('scrollmodechanged', () => {
			console.log("scrollmodechanged");
			var scroll = PDFViewerApplication.pdfViewer.scrollMode;
			if (scroll === this.scrollMode) this.start();
			else this.stop();
			var button = PDFViewerApplication.appConfig.secondaryToolbar.bookFlipButton;
			button.classList.toggle('toggled', scroll === this.scrollMode);
		});

		eventBus.on('switchspreadmode', (evt) => {
			console.log("switchspreadmode");
			this.spread(evt.mode);
			PDFViewerApplication.eventBus.dispatch('spreadmodechanged', {
				source: PDFViewerApplication,
				mode: evt.mode
			});
		});

		eventBus.on('pagesloaded', () => {
			console.log("pagesloaded");
			this._ready = true;
			if (this.toStart) {
				this.toStart = false;
				console.log("switch mode");
				PDFViewerApplication.pdfViewer.scrollMode = this.scrollMode;
				PDFViewerApplication.pdfViewer.currentScaleValue = "page-fit";
			}
		});
		eventBus.on(this.scrollMode == 4 ? 'pagerendered' : 'baseviewerinit', () => {
			console.log("baseviewerinit");
			PDFViewerApplicationOptions.set('scrollModeOnLoad', this.scrollMode);
			this._intoView = PDFViewerApplication.pdfViewer.scrollPageIntoView;
			this._visPages = PDFViewerApplication.pdfViewer._getVisiblePages;
		});
	},
	// startup flipbook
	start: function () {
		console.log("start");
		if (this.active || !this._ready) return;
		this.active = true;

		var viewer = PDFViewerApplication.pdfViewer;

		$('.scrollModeButtons').removeClass('toggled');

		this._spreadBk = viewer.spreadMode;
		var selected = $('.spreadModeButtons.toggled').attr('id');
		this._spread = (this._spreadBk !== 2) ? 0 : 2;
		viewer.spreadMode = 0;
		viewer._spreadMode = -1;
		$('.spreadModeButtons').removeClass('toggled');
		$('#' + selected).addClass('toggled');
		if (!/^#/.test(this.lName))
			this._evSpread = PDFViewerApplication.eventBus[this.lName].switchspreadmode;
		// PDFViewerApplication.eventBus._listeners.switchspreadmode = null;

		viewer.scrollPageIntoView = (data) => { return this.link(data) };
		viewer._getVisiblePages = () => { return this.load() };

		var scale = viewer.currentScale;
		var parent = this;

		$('#viewer').removeClass('pdfViewer').addClass('bookViewer').css({ opacity: 1 });
		$('#viewer .page').each(function () {
			parent._width[$(this).attr('data-page-number')] = $(this).width() / scale;
			parent._height[$(this).attr('data-page-number')] = $(this).height() / scale;
		});

		$('#spreadOdd').prop('disabled', true);
		var pages = PDFViewerApplication.pagesCount;
		for (var page = 3; page < pages + (pages % 2); page++) {
			if (this._height[page] != this._height[page - 1] || this._width[page] != this._width[page - 1]) {
				$('#spreadEven').prop('disabled', true);
				this._spread = 0;
			}
		}
		$('#viewer').turn({
			elevation: 50,
			width: this._size(PDFViewerApplication.page, 'width') * this._spreadMult(),
			height: this._size(PDFViewerApplication.page, 'height'),
			page: PDFViewerApplication.page,
			when: {
				turned: function (event, page) {
					PDFViewerApplication.page = page;
					console.log("update");
					viewer.update();
				}
			},
			display: this._spreadType()
		});
	},
	// shutdown flipbook
	stop: function () {
		console.log("stop");
		if (!this.active) return;
		this.active = false;

		var viewer = PDFViewerApplication.pdfViewer;

		$('#viewer').turn('destroy');

		viewer.scrollPageIntoView = this._intoView;
		if (this._visPages != null) viewer._getVisiblePages = this._visPages;
		if (!/^#/.test(this.lName))
			PDFViewerApplication.eventBus[this.lName].switchspreadmode = this._evSpread;
		viewer.spreadMode = this._spreadBk;

		$('#viewer .page').removeAttr('style');
		$('#viewer').removeAttr('style').removeClass('shadow bookViewer').addClass('pdfViewer');

		var parent = this;
		$('#viewer .page').each(function () {
			var page = $(this).attr('data-page-number');
			$(this).css('width', parent._size(page, 'width')).css('height', parent._size(page, 'height'));
		});

	},
	// resize flipbook pages
	resize: function () {
		console.log("resize");
		if (!this.active) return;
		if (this._spread !== 0 && document.getElementById("viewerContainer").getBoundingClientRect().width < 720) {
			this.spread(0);
		}
		if (this._spread === 0 && document.getElementById("viewerContainer").getBoundingClientRect().width > 720) {
			this.spread(1);
		}
		var page = PDFViewerApplication.page;
		$('#viewer').turn('size', this._size(page, 'width') * this._spreadMult(), this._size(page, 'height'));
	},
	// rotate flipbook pages
	rotate: function () {
		console.log("rotate");
		if (!this.active) return;
		[this._height, this._width] = [this._width, this._height];
		this.resize();
	},
	// change flipbook spread mode
	spread: function (spreadMode) {
		console.log("spread");
		if (!this.active) return;
		this._spread = spreadMode;
		$('#viewer').turn('display', this._spreadType());
		this.resize();
	},
	// turn page
	flip: function () {
		console.log("flip");
		if (!this.active) return;
		$('#viewer').turn('page', PDFViewerApplication.page);
		// force load next page
		// TODO: Find proper way to force this loading
		// this line seems to not do anything but successfully caused the website to update
		// it throws a lot of error in the console though. 
		//PDFViewerApplication.update();
		// end TODO
		if (!PDFViewerApplication.pdfViewer.hasEqualPageSizes) this.resize();
	},
	// follow internal links
	link: function (data) {
		console.log("link");
		if (!this.active) return;
		PDFViewerApplication.page = data.pageNumber;
	},
	// load pages near shown page
	load: function () {
		console.log("load");
		if (!this.active) return;
		var views = PDFViewerApplication.pdfViewer._pages;
		var arr = [];
		var page = PDFViewerApplication.page;
		var min = Math.max(page - ((this._spread === 0) ? 2 : 3 + (page % 2)), 0);
		var max = Math.min(page + ((this._spread === 0) ? 1 : 3 - (page % 2)), views.length);

		for (var i = min, ii = max; i < ii; i++) {
			arr.push({
				id: views[i].id,
				view: views[i],
				x: 0, y: 0, percent: 100
			});
		}

		return { first: arr[page - min - 1], last: arr[arr.length - 1], views: arr };
	},
	_spreadType: function () {
		return (this._spread === 0) ? 'single' : 'double';
	},
	_spreadMult: function () {
		return (this._spread === 0) ? 1 : 2;
	},
	_size: function (page, request) {
		var size;
		if (request === 'width') size = this._width[page];
		if (request === 'height') size = this._height[page];
		return size * PDFViewerApplication.pdfViewer.currentScale;
	}
};

//bookFlip.init();
