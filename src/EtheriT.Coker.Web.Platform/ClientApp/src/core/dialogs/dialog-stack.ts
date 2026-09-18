/** 彈窗堆疊：只有最上層能吃 Escape，全部關掉才解除捲動鎖。 */
const stack: symbol[] = [];

export function pushDialog(): symbol {
  const token = Symbol("dialog");
  stack.push(token);
  if (stack.length === 1) document.body.classList.add("dialog-open");
  return token;
}

export function popDialog(token: symbol): void {
  const index = stack.indexOf(token);
  if (index === -1) return;
  stack.splice(index, 1);
  if (stack.length === 0) document.body.classList.remove("dialog-open");
}

export function isTopDialog(token: symbol): boolean {
  return stack[stack.length - 1] === token;
}

export function hasOpenDialog(): boolean {
  return stack.length > 0;
}
