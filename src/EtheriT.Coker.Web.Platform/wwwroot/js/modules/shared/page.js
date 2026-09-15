export function getPageRoot(pageName) {
  return document.querySelector(`[data-platform-page="${pageName}"]`);
}
