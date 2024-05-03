function MapMessage() {
	$.loadCss("https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400..900;1,400..900&display=swap");
	if (typeof (window.counterUp) == "undefined") $.LoadJs("/lib/counterup2/index.js").done(setCounter);
	else setCounter();
	const show = function (e) {
		const $target = $(`${$(this).attr("href") || `#${$(this).data("area")}`}`);
		const top = window.scrollY;
		$(".MapMessage .active").removeClass("active");
		$target.addClass("active");
		$(`.MapMessage .mapData [data-Area="${$target.attr("id")}"]`).addClass("active");
		const counterUp = window.counterUp.default;

		$(".MapMessage .counter").each((i, el) => {
			counterUp(el, {
				duration: 500,
				delay: 16,
			})
		});
		return false;
	}
	$(".MapMessage map>area").on("click", show);
	$(".MapMessage map>area,.mapData li").on("mouseover", show);
}
function setCounter() {
	const counterUp = window.counterUp.default

	const callback = entries => {
		entries.forEach(entry => {
			const el = entry.target;
			if (entry.isIntersecting && !el.classList.contains('is-visible')) {
				counterUp(el, {
					duration: 2000,
					delay: 16,
				})
				el.classList.add('is-visible')
			}
		})
	}

	const IO = new IntersectionObserver(callback, { threshold: 1 })

	const el = document.querySelectorAll('.MapMessage .counter');
	el.forEach(entry => {
		 IO.observe(entry)
	});
}