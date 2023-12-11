function toggleContent() {
    var content = document.getElementById("content");
    content.classList.toggle("collapse");
    content.classList.toggle("expand");
}
var toggleLabel = document.querySelector('.toggle-label');
var toggleContent = document.querySelector('#toggle-content');

toggleLabel.addEventListener('click', function () {
    toggleContent.classList.toggle();
});
