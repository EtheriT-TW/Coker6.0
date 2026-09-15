function requireGlobal(name, value) {
  if (!value) {
    throw new Error(`${name} 尚未載入；請確認 vendor scripts 位於 Platform module 之前。`);
  }

  return value;
}

export function getDevExpress() {
  return requireGlobal("DevExpress", globalThis.DevExpress);
}

export function getJQuery() {
  return requireGlobal("jQuery", globalThis.jQuery);
}

export function initializeDevExtreme(locale) {
  const devExpress = getDevExpress();
  devExpress.localization.locale(locale);
}

export function getDataGrid(selectorOrElement) {
  const jquery = getJQuery();
  const element = typeof selectorOrElement === "string"
    ? document.querySelector(selectorOrElement)
    : selectorOrElement;

  return element ? jquery(element).dxDataGrid("instance") : null;
}
