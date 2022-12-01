import grapesjs from 'grapesjs';

export declare type PluginOptions = {
	/**
	 * Object to extend the default custom code block. Pass a falsy value to avoid adding the block
	 * @example
	 * { label: 'Custom Code', category: 'Extra', ... }
	 */
	blockCustomCode?: Partial<grapesjs.BlockOptions>;
	/**
	 * Object to extend the default custom code properties.
	 * @example
	 * { name: 'Custom Code', droppable: false, ... }
	 */
	propsCustomCode?: grapesjs.ComponentDefinition;
	/**
	 * Object to extend the default component's toolbar button for the code. Pass a falsy value to avoid adding the button
	 * @example
	 * { label: '</>', attributes: { title: 'Open custom code' } }
	 */
	toolbarBtnCustomCode?: Record<string, any>;
	/**
	 * Content to show when the custom code contains `<script>`
	 */
	placeholderScript?: string;
	/**
	 * Title for the custom code modal
	 * @default 'Insert your code'
	 */
	modalTitle?: string;
	/**
	 * Additional options for the code viewer.
	 * @example
	 * { theme: 'hopscotch', readOnly: 0 }
	 */
	codeViewOptions?: Record<string, any>;
	/**
	 * Label for the default save button
	 * @default 'Save'
	 */
	buttonLabel?: string;
	/**
	 * Object to extend the default custom code command.
	 */
	commandCustomCode?: Record<string, any>;
};
declare const plugin: grapesjs.Plugin<PluginOptions>;

export {
	plugin as default,
};

export {};
