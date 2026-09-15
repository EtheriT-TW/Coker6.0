export type SaveTrigger = "button" | "shortcut" | "programmatic";

export interface SavePipelineContext<TModel = unknown, TResult = unknown> {
  formId: string;
  trigger: SaveTrigger;
  values: TModel;
  result?: TResult;
  error?: unknown;
}

export interface SavePipelineHooks {
  beforeSave?(context: SavePipelineContext): boolean | void | Promise<boolean | void>;
  afterSave?(context: SavePipelineContext): void | Promise<void>;
  onSaveError?(context: SavePipelineContext): void | Promise<void>;
}

const hooks = new Set<SavePipelineHooks>();

export function registerSavePipelineHooks(saveHooks: SavePipelineHooks): () => void {
  hooks.add(saveHooks);
  return () => hooks.delete(saveHooks);
}

export async function runBeforeSaveHooks(context: SavePipelineContext): Promise<boolean> {
  for (const hook of hooks) {
    if (await hook.beforeSave?.(context) === false) return false;
  }
  return true;
}

export async function runAfterSaveHooks(context: SavePipelineContext): Promise<void> {
  for (const hook of hooks) await hook.afterSave?.(context);
}

export async function runSaveErrorHooks(context: SavePipelineContext): Promise<void> {
  for (const hook of hooks) await hook.onSaveError?.(context);
}
