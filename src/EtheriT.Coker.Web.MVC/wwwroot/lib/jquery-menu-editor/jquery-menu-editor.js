/**
 * jQuery Menu Editor
 * @author David Ticona Saravia https://github.com/davicotico
 * @version 1.1.0
 * */
( function( $ )
{    
    /**
     * @desc jQuery plugin to sort html list also the tree structures
     * @version 1.4.0
     * @author Vladimír Čamaj
     * @license MIT
     * @desc jQuery plugin
     * @param options
     * @returns this to unsure chaining
     */
    $.fn.sortableLists = function( options )
    {
        // Local variables. This scope is available for all the functions in this closure.
        var jQBody = $( 'body' ).css( 'position', 'relative' ),

            defaults = {
                currElClass: '',
                placeholderClass: '',
                placeholderCss: {
                    'position': 'relative',
                    'padding': 0
                },
                hintClass: '',
                hintCss: {
                    'display': 'none',
                    'position': 'relative',
                    'padding': 0
                },
                hintWrapperClass: '',
                hintWrapperCss: { /* Description is below the defaults in this var section */ },
                baseClass: '',
                baseCss: {
                    'position': 'absolute',
                    'top': 0 - parseInt( jQBody.css( 'margin-top' ) ),
                    'left': 0 - parseInt( jQBody.css( 'margin-left' ) ),
                    'margin': 0,
                    'padding': 0,
                    'z-index': 2500
                },
                opener: {
                    active: false,
                    open: '',
                    close: '',
                    openerCss: {
                        'float': 'left',
                        'display': 'inline-block',
                        'background-position': 'center center',
                        'background-repeat': 'no-repeat'
                    },
                    openerClass: ''
                },
                listSelector: 'ul',
                listsClass: '', // Used for hintWrapper and baseElement
                listsCss: {},
                insertZone: 50,
                insertZonePlus: false,
                scroll: 20,
                ignoreClass: '',
                isAllowed: function( cEl, hint, target ) { return true; },  // Params: current el., hint el.
                onDragStart: function( e, cEl ) { return true; },  // Params: e jQ. event obj., current el.
                onChange: function( cEl ) { return true; },  // Params: current el.
                complete: function( cEl ) { return true; }  // Params: current el.
            },

            setting = $.extend( true, {}, defaults, options ),

            // base element from which is counted position of draged element
            base = $( '<' + setting.listSelector + ' />' )
                .prependTo( jQBody )
                .attr( 'id', 'sortableListsBase' )
                .css( setting.baseCss )
                .addClass( setting.listsClass + ' ' + setting.baseClass ),

            // placeholder != state.placeholderNode
            // placeholder is document fragment and state.placeholderNode is document node
            placeholder = $( '<li />' )
                .attr( 'id', 'sortableListsPlaceholder' )
                .css( setting.placeholderCss )
                .addClass( setting.placeholderClass ),

            // hint is document fragment
            hint = $( '<li />' )
                .attr( 'id', 'sortableListsHint' )
                .css( setting.hintCss )
                .addClass( setting.hintClass ),

            // Is document fragment used as wrapper if hint is inserted to the empty li
            hintWrapper = $( '<' + setting.listSelector + ' />' )
                .attr( 'id', 'sortableListsHintWrapper' )
                .addClass( setting.listsClass + ' ' + setting.hintWrapperClass )
                .css( setting.listsCss )
                .css( setting.hintWrapperCss ),

            // Is +/- ikon to open/close nested lists
            opener = $( '<span />' )
                .addClass( 'sortableListsOpener ' + setting.opener.openerClass )
                .css( setting.opener.openerCss )
                .on( 'mousedown touchstart', function( e )
                {
                    var li = $( this ).closest( 'li' );

                    if ( li.hasClass( 'sortableListsClosed' ) )
                    {
                        open( li );
                    }
                    else
                    {
                        close( li );
                    }

                    return false; // Prevent default
                } );

        if ( setting.opener.as == 'class' )
        {
            opener.addClass( setting.opener.close );
        }
        else if ( setting.opener.as == 'html' )
        {
            opener.html( setting.opener.close );
        }
        else
        {
            console.error('Invalid setting for opener.as');
        }

        // Container with all actual elements and parameters
        var state = {
            isDragged: false,
            isRelEFP: null,  // How browser counts elementFromPoint() position (relative to window/document)
            oEl: null, // overElement is element which returns elementFromPoint() method
            rootEl: null,
            cEl: null, // currentElement is currently dragged element
            upScroll: false,
            downScroll: false,
            pX: 0,
            pY: 0,
            cX: 0,
            cY: 0,
            isAllowed: true, // The function is defined in setting
            e: { pageX: 0, pageY: 0, clientX: 0, clientY: 0 }, // TODO: unused??
            doc: $( document ),
            win: $( window )
        };

        if ( setting.opener.active )
        {
            if ( ! setting.opener.open ) throw 'Opener.open value is not defined. It should be valid url, html or css class.';
            if ( ! setting.opener.close ) throw 'Opener.close value is not defined. It should be valid url, html or css class.';

            $( this ).find( 'li' ).each( function()
            {
                var li = $( this );

                if ( li.children( setting.listSelector ).length )
                {
                    opener.clone( true ).prependTo( li.children( 'div' ).first() );

                    if ( ! li.hasClass( 'sortableListsOpen' ) )
                    {
                        close( li );
                    }
                    else
                    {
                        open( li );
                    }
                }
            } );
        }

        // Return this ensures chaining
        return this.on( 'mousedown touchstart', function( e )
            {
                var target = $( e.target );

                if ( state.isDragged !== false || ( setting.ignoreClass && target.hasClass( setting.ignoreClass ) ) ) return; // setting.ignoreClass is checked cause hasClass('') returns true

                // Solves selection/range highlighting
                e.preventDefault();

                if ( e.type === 'touchstart' )
                {
                    setTouchEvent( e );
                }

                // El must be li in jQuery object
                var el = target.closest( 'li' ),
                    rEl = $( this );

                // Check if el is not empty
                if ( el[ 0 ] )
                {
                    setting.onDragStart( e, el );
                    startDrag( e, el, rEl );
                }
            }
        );

        /**
         * @desc Binds events dragging and endDrag, sets some init. values
         * @param e event obj.
         * @param el curr. dragged element
         * @param rEl root element
         */
        function startDrag( e, el, rEl )
        {
            state.isDragged = true;

            var elMT = parseInt( el.css( 'margin-top' ) ), // parseInt is necesary cause value has px at the end
                elMB = parseInt( el.css( 'margin-bottom' ) ),
                elML = parseInt( el.css( 'margin-left' ) ),
                elMR = parseInt( el.css( 'margin-right' ) ),
                elXY = el.offset(),
                elIH = el.innerHeight();

            state.rootEl = {
                el: rEl,
                offset: rEl.offset(),
                rootElClass: rEl.attr( 'class' )
            };

            state.cEl = {
                el: el,
                mT: elMT, mL: elML, mB: elMB, mR: elMR,
                offset: elXY
            };

            state.cEl.xyOffsetDiff = { X: e.pageX - state.cEl.offset.left, Y: e.pageY - state.cEl.offset.top };
            state.cEl.el.addClass( 'sortableListsCurrent' + ' ' + setting.currElClass );

            el.before( placeholder );  // Now document has node placeholder

            var placeholderNode = state.placeholderNode = $( '#sortableListsPlaceholder' );  // jQuery object && document node

            el.css( {
                'width': el.width(),
                'position': 'absolute',
                'top': elXY.top - elMT,
                'left': elXY.left - elML
            } ).prependTo( base );

            placeholderNode.css( {
                'display': 'block',
                'height': elIH
            } );

            hint.css( 'height', elIH );

            state.doc
                .on( 'mousemove touchmove', dragging )
                .on( 'mouseup touchend touchcancel', endDrag );

        }

        /**
         * @desc Start dragging
         * @param e event obj.
         */
        function dragging( e )
        {
            if ( state.isDragged )
            {
                var cEl = state.cEl,
                    doc = state.doc,
                    win = state.win;

                if ( e.type === 'touchmove' )
                {
                    setTouchEvent( e );
                }

                // event triggered by trigger() from setInterval does not have XY properties
                if ( ! e.pageX )
                {
                    setEventPos( e );
                }

                // Scrolling up
                if ( doc.scrollTop() > state.rootEl.offset.top - 10 && e.clientY < 50 )
                {
                    if ( ! state.upScroll ) // Has to be here after cond. e.clientY < 50 cause else unsets the interval
                    {
                        setScrollUp( e );
                    }
                    else
                    {
                        e.pageY = e.pageY - setting.scroll;
                        $( 'html, body' ).each( function( i )
                        {
                            $( this ).scrollTop( $( this ).scrollTop() - setting.scroll );
                        } );
                        setCursorPos( e );
                    }
                }
                // Scrolling down
                else if ( doc.scrollTop() + win.height() < state.rootEl.offset.top + state.rootEl.el.outerHeight( false ) + 10 && win.height() - e.clientY < 50 )
                {
                    if ( ! state.downScroll )
                    {
                        setScrollDown( e );
                    }
                    else
                    {
                        e.pageY = e.pageY + setting.scroll;
                        $( 'html, body' ).each( function( i )
                        {
                            $( this ).scrollTop( $( this ).scrollTop() + setting.scroll );
                        } );
                        setCursorPos( e );
                    }
                }
                else
                {
                    scrollStop( state );
                }

                // Script needs to know old oEl
                state.oElOld = state.oEl;

                cEl.el[ 0 ].style.visibility = 'hidden';  // This is important for the next row
                state.oEl = oEl = elFromPoint( e.pageX, e.pageY );
                cEl.el[ 0 ].style.visibility = 'visible';

                showHint( e, state );

                setCElPos( e, state );

            }
        }

        /**
         * @desc endDrag unbinds events mousemove/mouseup and removes redundant elements
         * @param e
         */
        function endDrag( e )
        {
            var cEl = state.cEl,
                hintNode = $( '#sortableListsHint', state.rootEl.el ),
                hintStyle = hint[ 0 ].style,
                targetEl = null, // hintNode/placeholderNode
                isHintTarget = false, // if cEl will be placed to the hintNode
                hintWrapperNode = $( '#sortableListsHintWrapper' );

            if ( e.type === 'touchend' || e.type === 'touchcancel' )
            {
                setTouchEvent( e );
            }

            if ( hintStyle.display == 'block' && hintNode.length && state.isAllowed )
            {
                targetEl = hintNode;
                isHintTarget = true;
            }
            else
            {
                targetEl = state.placeholderNode;
                isHintTarget = false;
            }

            offset = targetEl.offset();

            cEl.el.animate( { left: offset.left - state.cEl.mL, top: offset.top - state.cEl.mT }, 250,
                function()  // complete callback
                {
                    tidyCurrEl( cEl );

                    targetEl.after( cEl.el[ 0 ] );
                    targetEl[ 0 ].style.display = 'none';
                    hintStyle.display = 'none';
                    // This have to be document node, not hint as a part of documentFragment.
                    hintNode.remove();

                    hintWrapperNode
                        .removeAttr( 'id' )
                        .removeClass( setting.hintWrapperClass );

                    if ( hintWrapperNode.length )
                    {
                        //hintWrapperNode.prev( 'div' ).append( opener.clone( true ) ); // original
                        hintWrapperNode.prev( 'div' ).prepend( opener.clone( true ) ); //david
                    }

                    // Directly removed placeholder looks bad. It jumps up if the hint is below.
                    if ( isHintTarget )
                    {
                        state.placeholderNode.slideUp( 150, function()
                        {
                            state.placeholderNode.remove();
                            tidyEmptyLists();
                            setting.onChange( cEl.el );
                            setting.complete( cEl.el ); // Have to be here cause is necessary to remove placeholder before complete call.
                            state.isDragged = false;
                        } );
                    }
                    else
                    {
                        state.placeholderNode.remove();
                        tidyEmptyLists();
                        setting.complete( cEl.el );
                        state.isDragged = false;
                    }

                } );

            scrollStop( state );

            state.doc
                .unbind( "mousemove touchmove", dragging )
                .unbind( "mouseup touchend touchcancel", endDrag );


        }

        //////// Helpers /////////////////////////////////////////////////////////////////////////////////////

        //////// Scroll handlers /////////////////////////////////////////////////////////////////////////////

        /**
         * @desc Ensures autoscroll up.
         * @param e
         * @return No value
         */
        function setScrollUp( e )
        {
            if ( state.upScroll ) return;

            state.upScroll = setInterval( function()
            {
                state.doc.trigger( 'mousemove' );
            }, 50 );

        }

        /**
         * @desc Ensures autoscroll down.
         * @param e
         * @return No value
         */
        function setScrollDown( e )
        {
            if ( state.downScroll ) return;

            state.downScroll = setInterval( function()
            {
                state.doc.trigger( 'mousemove' );
            }, 50 );

        }

        /**
         * @desc This properties are used when setScrollUp()/Down() calls trigger('mousemove'), cause trigger() produce event object without pageY/Y and clientX/Y.
         * @param e
         * @return No value
         */
        function setCursorPos( e )
        {
            state.pY = e.pageY;
            state.pX = e.pageX;
            state.cY = e.clientY;
            state.cX = e.clientX;
        }

        /**
         * @desc Necessary while scrolling, cause trigger('mousemove') does not set cursor XY values in event object
         * @param e
         * @return No value
         */
        function setEventPos( e )
        {
            e.pageY = state.pY;
            e.pageX = state.pX;
            e.clientY = state.cY;
            e.clientX = state.cX;
        }

        /**
         * @desc Stops scrolling and sets variables
         * @param state
         * @return No value
         */
        function scrollStop( state )
        {
            clearInterval( state.upScroll );
            clearInterval( state.downScroll );
            // clearInterval have to be before upScroll/downScroll is set to false
            state.upScroll = state.downScroll = false;
        }

        /////// End of Scroll handlers //////////////////////////////////////////////////////////////
        /////// Current element handlers //////////////////////////////////////////////////////////////

        /**
         * Sets the e.page/e.screen properties
         * @param e
         */
        function setTouchEvent( e )
        {
            e.pageX = e.originalEvent.changedTouches[ 0 ].pageX;
            e.pageY = e.originalEvent.changedTouches[ 0 ].pageY;
            e.screenX = e.originalEvent.changedTouches[ 0 ].screenX;
            e.screenY = e.originalEvent.changedTouches[ 0 ].screenY;
        }

        /**
         * @desc Sets the position of dragged element
         * @param e event object
         * @param state state object
         * @return No value
         */
        function setCElPos( e, state )
        {
            var cEl = state.cEl;

            cEl.el.css( {
                'top': e.pageY - cEl.xyOffsetDiff.Y - cEl.mT,
                'left': e.pageX - cEl.xyOffsetDiff.X - cEl.mL
            } )

        }

        /**
         * @desc Return elementFromPoint() result as jQuery object
         * @param x e.pageX
         * @param y e.pageY
         * @return null|jQuery object
         */
        function elFromPoint( x, y )
        {
            if ( ! document.elementFromPoint ) return null;

            // FF/IE/CH needs coordinates relative to the window, unlike
            // Opera/Safari which needs absolute coordinates of document in elementFromPoint()
            var isRelEFP = state.isRelEFP;

            // isRelative === null means it is not checked yet
            if ( isRelEFP === null )
            {
                var s, res;
                if ( (s = state.doc.scrollTop()) > 0 )
                {
                    isRelEFP = ( (res = document.elementFromPoint( 0, s + $( window ).height() - 1 ) ) == null
                    || res.tagName.toUpperCase() == 'HTML');  // IE8 returns html
                }
                if ( (s = state.doc.scrollLeft()) > 0 )
                {
                    isRelEFP = ( (res = document.elementFromPoint( s + $( window ).width() - 1, 0 ) ) == null
                    || res.tagName.toUpperCase() == 'HTML');  // IE8 returns html
                }
            }

            if ( isRelEFP )
            {
                x -= state.doc.scrollLeft();
                y -= state.doc.scrollTop();
            }

            // Returns jQuery object
            var el = $( document.elementFromPoint( x, y ) );

            if ( ! state.rootEl.el.find( el ).length ) // el is outside the rootEl
            {
                return null;
            }
            else if ( el.is( '#sortableListsPlaceholder' ) || el.is( '#sortableListsHint' ) ) // el is #placeholder/#hint
            {
                return null;
            }
            else if ( ! el.is( 'li' ) ) // el is ul or div or something else in li elem.
            {
                el = el.closest( 'li' );
                return el[ 0 ] ? el : null;
            }
            else if ( el.is( 'li' ) ) // el is most wanted li
            {
                return el;
            }

        }
        //////// End of current element handlers //////////////////////////////////////////////////////
        //////// Show hint handlers //////////////////////////////////////////////////////

        /**
         * @desc Shows or hides or does not show hint element
         * @param e event
         * @param state
         * @return No value
         */
        function showHint( e, state )
        {
            var oEl = state.oEl;

            // If oEl is null or if this is the first call in dragging
            if ( ! oEl || ! state.oElOld )  return;

            var oElH = oEl.outerHeight( false ),
                relY = e.pageY - oEl.offset().top;

            if ( setting.insertZonePlus )
            {
                if ( 14 > relY )  // Inserting on top
                {
                    showOnTopPlus( e, oEl, 7 > relY );  // Last bool param express if hint insert outside/inside
                }
                else if ( oElH - 14 < relY )  // Inserting on bottom
                {
                    showOnBottomPlus( e, oEl, oElH - 7 < relY );
                }
            }
            else
            {
                if ( 5 > relY )  // Inserting on top
                {
                    showOnTop( e, oEl );
                }
                else if ( oElH - 5 < relY )  // Inserting on bottom
                {
                    showOnBottom( e, oEl );
                }
            }
        }

        /**
         * @desc Called from showHint method. Displays or hides hint element
         * @param e event
         * @param oEl oElement
         * @return No value
         */
        function showOnTop( e, oEl )
        {
            if ( $( '#sortableListsHintWrapper', state.rootEl.el ).length )
            {
                hint.unwrap();  // If hint is wrapped by ul/ol #sortableListsHintWrapper
            }

            // Hint outside the oEl
            if ( e.pageX - oEl.offset().left < setting.insertZone )
            {
                // Ensure display:none if hint will be next to the placeholder
                if ( oEl.prev( '#sortableListsPlaceholder' ).length )
                {
                    hint.css( 'display', 'none' );
                    return;
                }
                oEl.before( hint );
            }
            // Hint inside the oEl
            else
            {
                var children = oEl.children(),
                    list = oEl.children( setting.listSelector ).first();

                if ( list.children().first().is( '#sortableListsPlaceholder' ) )
                {
                    hint.css( 'display', 'none' );
                    return;
                }

                // Find out if is necessary to wrap hint by hintWrapper
                if ( ! list.length )
                {
                    children.first().after( hint );
                    hint.wrap( hintWrapper );
                }
                else
                {
                    list.prepend( hint );
                }

                if ( state.oEl )
                {
                    open( oEl ); // TODO:animation??? .children('ul,ol').css('display', 'block');
                }

            }

            hint.css( 'display', 'block' );
            // Ensures posible formating of elements. Second call is in the endDrag method.
            state.isAllowed = setting.isAllowed( state.cEl.el, hint, hint.parents( 'li' ).first() );

        }

        /**
         * @desc Called from showHint method. Displays or hides hint element
         * @param e event
         * @param oEl oElement
         * @param outside bool
         * @return No value
         */
        function showOnTopPlus( e, oEl, outside )
        {
            if ( $( '#sortableListsHintWrapper', state.rootEl.el ).length )
            {
                hint.unwrap();  // If hint is wrapped by ul/ol #sortableListsHintWrapper
            }

            // Hint inside the oEl
            if ( ! outside && e.pageX - oEl.offset().left > setting.insertZone )
            {
                var children = oEl.children(),
                    list = oEl.children( setting.listSelector ).first();

                if ( list.children().first().is( '#sortableListsPlaceholder' ) )
                {
                    hint.css( 'display', 'none' );
                    return;
                }

                // Find out if is necessary to wrap hint by hintWrapper
                if ( ! list.length )
                {
                    children.first().after( hint );
                    hint.wrap( hintWrapper );
                }
                else
                {
                    list.prepend( hint );
                }

                if ( state.oEl )
                {
                    open( oEl ); // TODO:animation??? .children('ul,ol').css('display', 'block');
                }
            }
            // Hint outside the oEl
            else
            {
                // Ensure display:none if hint will be next to the placeholder
                if ( oEl.prev( '#sortableListsPlaceholder' ).length )
                {
                    hint.css( 'display', 'none' );
                    return;
                }
                oEl.before( hint );

            }

            hint.css( 'display', 'block' );
            // Ensures posible formating of elements. Second call is in the endDrag method.
            state.isAllowed = setting.isAllowed( state.cEl.el, hint, hint.parents( 'li' ).first() );

        }

        /**
         * @desc Called from showHint function. Displays or hides hint element.
         * @param e event
         * @param oEl oElement
         * @return No value
         */
        function showOnBottom( e, oEl )
        {
            if ( $( '#sortableListsHintWrapper', state.rootEl.el ).length )
            {
                hint.unwrap();  // If hint is wrapped by ul/ol sortableListsHintWrapper
            }

            // Hint outside the oEl
            if ( e.pageX - oEl.offset().left < setting.insertZone )
            {
                // Ensure display:none if hint will be next to the placeholder
                if ( oEl.next( '#sortableListsPlaceholder' ).length )
                {
                    hint.css( 'display', 'none' );
                    return;
                }
                oEl.after( hint );
            }
            // Hint inside the oEl
            else
            {
                var children = oEl.children(),
                    list = oEl.children( setting.listSelector ).last();  // ul/ol || empty jQuery obj

                if ( list.children().last().is( '#sortableListsPlaceholder' ) )
                {
                    hint.css( 'display', 'none' );
                    return;
                }

                // Find out if is necessary to wrap hint by hintWrapper
                if ( list.length )
                {
                    children.last().append( hint );
                }
                else
                {
                    oEl.append( hint );
                    hint.wrap( hintWrapper );
                }

                if ( state.oEl )
                {
                    open( oEl ); // TODO: animation???
                }

            }

            hint.css( 'display', 'block' );
            // Ensures posible formating of elements. Second call is in the endDrag method.
            state.isAllowed = setting.isAllowed( state.cEl.el, hint, hint.parents( 'li' ).first() );

        }

        /**
         * @desc Called from showHint function. Displays or hides hint element.
         * @param e event
         * @param oEl oElement
         * @param outside bool
         * @return No value
         */
        function showOnBottomPlus( e, oEl, outside )
        {
            if ( $( '#sortableListsHintWrapper', state.rootEl.el ).length )
            {
                hint.unwrap();  // If hint is wrapped by ul/ol sortableListsHintWrapper
            }

            // Hint inside the oEl
            if ( ! outside && e.pageX - oEl.offset().left > setting.insertZone )
            {
                var children = oEl.children(),
                    list = oEl.children( setting.listSelector ).last();  // ul/ol || empty jQuery obj

                if ( list.children().last().is( '#sortableListsPlaceholder' ) )
                {
                    hint.css( 'display', 'none' );
                    return;
                }

                // Find out if is necessary to wrap hint by hintWrapper
                if ( list.length )
                {
                    children.last().append( hint );
                }
                else
                {
                    oEl.append( hint );
                    hint.wrap( hintWrapper );
                }

                if ( state.oEl )
                {
                    open( oEl ); // TODO: animation???
                }

            }
            // Hint outside the oEl
            else
            {
                // Ensure display:none if hint will be next to the placeholder
                if ( oEl.next( '#sortableListsPlaceholder' ).length )
                {
                    hint.css( 'display', 'none' );
                    return;
                }
                oEl.after( hint );

            }

            hint.css( 'display', 'block' );
            // Ensures posible formating of elements. Second call is in the endDrag method.
            state.isAllowed = setting.isAllowed( state.cEl.el, hint, hint.parents( 'li' ).first() );

        }

        //////// End of show hint handlers ////////////////////////////////////////////////////
        //////// Open/close handlers //////////////////////////////////////////////////////////

        /**
         * @desc Handles opening nested lists
         * @param li
         */
        function open( li )
        {
            li.removeClass( 'sortableListsClosed' ).addClass( 'sortableListsOpen' );
            li.children( setting.listSelector ).css( 'display', 'block' );

            var opener = li.children( 'div' ).children( '.sortableListsOpener' ).first();

            if ( setting.opener.as == 'html' )
            {
                opener.html( setting.opener.close );
            }
            else if ( setting.opener.as == 'class' )
            {
                opener.addClass( setting.opener.close ).removeClass( setting.opener.open );
            }
            else
            {
                opener.css( 'background-image', 'url(' + setting.opener.close + ')' );
            }
        }

        /**
         * @desc Handles opening nested lists
         * @param li
         */
        function close( li )
        {
            li.removeClass( 'sortableListsOpen' ).addClass( 'sortableListsClosed' );
            li.children( setting.listSelector ).css( 'display', 'none' );

            var opener = li.children( 'div' ).children( '.sortableListsOpener' ).first();

            if ( setting.opener.as == 'html' )
            {
                opener.html( setting.opener.open );
            }
            else if ( setting.opener.as == 'class' )
            {
                opener.addClass( setting.opener.open ).removeClass( setting.opener.close );
            }
            else
            {
                opener.css( 'background-image', 'url(' + setting.opener.open + ')' );
            }

        }

        /////// Enf of open/close handlers //////////////////////////////////////////////

        /**
         * @desc Places the currEl to the target place
         * @param cEl
         */
        function tidyCurrEl( cEl )
        {
            var cElStyle = cEl.el[ 0 ].style;

            cEl.el.removeClass( setting.currElClass + ' ' + 'sortableListsCurrent' );
            cElStyle.top = '0';
            cElStyle.left = '0';
            cElStyle.position = 'relative';
            cElStyle.width = 'auto';

        }

        /**
         * @desc Removes empty lists and redundant openers
         */
        function tidyEmptyLists()
        {
            // Remove every empty ul/ol from root and also with .sortableListsOpener
            // hintWrapper can not be removed before the hint
            $( setting.listSelector, state.rootEl.el ).each( function( i )
                {
                    if ( ! $( this ).children().length )
                    {
                        $( this ).prev( 'div' ).children( '.sortableListsOpener' ).first().remove();
                        $( this ).remove();
                    }
                }
            );

        }

    };

    /** END PLUGIN sortableLists */

    /**
     * @desc Handles opening nested lists
     * @param setting
     */
    $.fn.iconOpen = function(setting){
        this.removeClass('sortableListsClosed').addClass('sortableListsOpen');
        this.children('ul').css('display', 'block');
        var opener = this.find('.float-left').children('.sortableListsOpener').first();
        if (setting.opener.as === 'html'){
            opener.html(setting.opener.close);
        } else if (setting.opener.as === 'class') {
            opener.addClass(setting.opener.close).removeClass(setting.opener.open);
        }
    };
    /**
     * @desc Handles closing nested lists
     * @param setting
     */
    $.fn.iconClose = function(setting) {
        this.removeClass('sortableListsOpen').addClass('sortableListsClosed');
        this.children('ul').css('display', 'none');
        var opener = this.find('.float-left').children('.sortableListsOpener').first();
        if (setting.opener.as === 'html') {
            opener.html(setting.opener.open);
        } else if (setting.opener.as === 'class') {
            opener.addClass(setting.opener.open).removeClass(setting.opener.close);
        }
    };
    
    /**
     * @author David Ticona Saravia
     * @desc Get the json from html list
     * @return {array} Array
     */
    $.fn.sortableListsToJson = function (){
        var arr = [];
        $(this).children('li').each(function () {
            var li = $(this);
            var object = li.data();
            arr.push(object);
            var ch = li.children('ul,ol').sortableListsToJson();
            if (ch.length > 0) {
                object.children = ch;
            } else {
                delete object.children;
            }
        });
        return arr;
    };

    /**
     * Update levels on <ul> data attribute 
     */
    $.fn.updateLevels = function(depth){
        var level = (typeof depth === 'undefined') ? 0 : depth;
        $(this).children('li').each(function () {
            var li = $(this);
            var ch = li.children('ul');
            if (ch.length > 0) {
                ch.data("level", level + 1);
                ch.updateLevels(level + 1);
            }
        });
    };

    /**
     * @description Update the buttons at the nested list (the main <ul>).
     * the buttons are: up, down, item in, item out
     * @param {int} depth 
     */
    $.fn.updateButtons = function (depth){
        var level = (typeof depth === 'undefined') ? 0 : depth;
        var removefirst = ['Up', 'In'];
        var removelast = ['Down'];
        if (level===0){
            removefirst.push('Out');
            removelast.push('Out');
            $(this).children('li').hideButtons(['Out']);
        }
        $(this).children('li').each(function () {
            var $li = $(this);
            var $ul = $li.children('ul');
            if ($ul.length > 0) {
                $ul.updateButtons(level + 1);
            }
        }); 
        $(this).children('li:first').hideButtons(removefirst);
        $(this).children('li:last').hideButtons(removelast);
    };
    /**
     * @description Hide the buttons at the item <li>
     * @param {Array} buttons 
     */
    $.fn.hideButtons = function(buttons){
        for(var i = 0; i<buttons.length; i++){
            $(this).find('.btn-group:first').children(".btn"+buttons[i]).hide();
        }
    };
}(jQuery));
/**
 * @version 1.1.0
 * @author David Ticona Saravia
 * @param {string} idSelector Attr ID
 * @param {object} options Options editor
 * */
function MenuEditor(idSelector, options) {
    var self = this;
    var $main = $("#" + idSelector).data("level", "0");
    var lastcheck = true;
    var settings = {
        labelEdit: '<i class="fas fa-edit clickable"></i>',
        labelRemove: '<i class="fas fa-trash-alt clickable"></i>',
        textConfirmDelete: 'This item {0} will be deleted. Are you sure?',
        iconPicker: { cols: 4, rows: 4, footer: false, iconset: "GoogleMaterialSymbolsOutlined" },
        maxLevel: -1,
        levelChang: true,
        listOptions: { 
            hintCss: { border: '1px dashed #13981D'}, 
            opener: {
                as: 'html',
                close: '<i class="fas fa-minus"></i>',
                open: '<i class="fas fa-plus"></i>',
                openerCss: {'margin-right': '10px', 'float': 'none'},
                openerClass: 'btn btn-outline-success btn-sm',
            },
            placeholderCss: {'background-color': 'gray'},
            ignoreClass: 'clickable',
            listsClass: "pl-0",
            listsCss: {"padding-top": "10px"},
            complete: function () {
                if (!lastcheck) co.sweet.error("操作錯誤", "該項目無法至此階層。");
                lastcheck = true;
                return true;
            },
            onChange: function (cEl) {
                self.updateButtons($main);
                $main.updateLevels(0);
                !!settings.on.drop && settings.on.drop(cEl);
            },
            isAllowed: function (currEl, hint, target) {
                lastcheck = isValidLevel(currEl, target);
                return lastcheck;
            }
        },
        on: {},
        btn:[]
    };
    options = options || {};
    /* STATIC METHOD */
    /**
     * Update the buttons on the list. Only the buttons 'Up', 'Down', 'In', 'Out'
     * @param {jQuery} $mainList The unorder list 
     **/

    self.updateButtons = function ($mainList) {
        $mainList.find('.btnMove').show();
        if(!settings.levelChang) $mainList.find('.levelMove').hide();
        $mainList.updateButtons();
    };
    self._moveLocked = true;
    var dragGuard = function (e) {
        if (!self._moveLocked) return;
        var $t = $(e.target);
        if ($t.closest('.clickable').length) return; // 讓功能按鈕可點
        e.stopImmediatePropagation();
        e.preventDefault();
        return false;
    };
    $main.off('mousedown.toggleMoveLock touchstart.toggleMoveLock')
        .on('mousedown.toggleMoveLock touchstart.toggleMoveLock', 'li, li *', dragGuard);
    function applyMoveLockUI() {
        var locked = !!self._moveLocked;

        // 停用/恢復「上下移/入/出階層」按鈕
        var $moveBtns = $main.find('.btnMove');
        if (locked) {
            $moveBtns.attr({ 'aria-disabled': 'true', 'tabindex': '-1' }).addClass('is-move-locked');
        } else {
            $moveBtns.removeAttr('aria-disabled tabindex').removeClass('is-move-locked');
        }

        // 容器標記
        $main.toggleClass('menu-move-locked', locked);

        // 讓既有規則重新計算顯示
        if (self.updateButtons) self.updateButtons($main);
    }

    self.toggleMoveLock = function (force) {
        self._moveLocked = (typeof force === 'boolean') ? force : !self._moveLocked;
        applyMoveLockUI();
        // 更新外部控制按鈕圖示（若有）
        if (self._moveLockButton && self._renderMoveLockBtn) {
            self._renderMoveLockBtn(self._moveLocked);
        }
        return self._moveLocked;
    };

    self.lockMove = function () { return self.toggleMoveLock(true); };
    self.unlockMove = function () { return self.toggleMoveLock(false); };
    var $moveBtn = $(options.element.moveEnable);

    function _norm(v) { return (v == null ? "" : String(v)); }
    function _getPath($li) { // 備援：路徑索引
        var path = [], $cur = $li;
        while ($cur && $cur.length) {
            path.unshift($cur.prevAll('li').length);
            $cur = $cur.parent('ul').parent('li');
            if (!$cur.length) break;
        }
        return path;
    }
    function _cssEscape(sel) { // 最低限度的 selector escape
        return (window.CSS && CSS.escape) ? CSS.escape(sel) : sel.replace(/([^a-zA-Z0-9_\-:])/g, "\\$1");
    }
    // 依 data.Id（或 id），若沒有就用索引路徑，產生 DOM id；並寫回到 li
    function _ensureDomId($li) {
        var rid = ($main.attr('id') || 'menu').replace(/[^\w\-:.]/g, '_'); // 根容器前綴，避免全域碰撞
        var key = _norm($li.data('Id') || $li.data('id'));
        var domId = key ? `mitem-${rid}-${key}` : `mitem-${rid}-path-${_getPath($li).join('-')}`;
        $li.attr('id', domId);
        return domId;
    }
    // 重建後把所有 li 補上（或更新）id
    function _tagAllDomIds() {
        $main.find('li').each(function () { _ensureDomId($(this)); });
    }
    
    $.extend(true, settings, options);

    var itemEditing = null;
    var sortableReady = true;
    var $form = null;
    var $updateButton = null;
    var iconPickerOpt = settings.iconPicker;
    var options = settings.listOptions;
    var iconPicker = $('#'+idSelector+'_icon').iconpicker(iconPickerOpt);
    $main.sortableLists(settings.listOptions);

    /* EVENTS */
    if ($moveBtn.length > 0) {
        var iconUnlocked =
            '<span class="fa-stack" aria-hidden="true" style="line-height:1;">' +
            '<i class="fa-solid fa-up-down-left-right fa-stack-1x"></i>' +
            '<i class="fa-solid fa-circle-notch fa-stack-2x"></i>' +
            '</span>';
        var iconLocked =
            '<span class="fa-stack" aria-hidden="true" style="line-height:1;">' +
            '<i class="fa-solid fa-up-down-left-right fa-stack-1x"></i>' +
            '<i class="fa-solid fa-ban fa-stack-2x"></i>' +
            '</span>';
        self._moveLockButton = $moveBtn
        self._renderMoveLockBtn = function (locked) {
            $moveBtn
                .attr({
                    'type': $moveBtn.attr('type') || 'button',
                    'aria-pressed': (!locked).toString(),
                    'aria-label': locked ? '移動鎖定（點擊解鎖）' : '移動解鎖（點擊鎖定）',
                    'title': locked ? '移動鎖定' : '移動解鎖'
                })
                .toggleClass('is-locked', locked)
                .html(locked ? iconLocked : iconUnlocked);
        }
        ;
        $moveBtn.off('.moveLock').on('click.moveLock', function (e) {
            e.preventDefault();
            self.toggleMoveLock();
        });
        self._moveLocked = true;
        self._renderMoveLockBtn(true);
        applyMoveLockUI();

        if (window.MutationObserver) {
            var mo = new MutationObserver(function (mutations) {
                if (!self._moveLocked) return;
                var needApply = false;
                mutations.forEach(function (m) {
                    // 有新增節點、且裡面可能出現 li / .btnMove 就補套
                    if (m.addedNodes && m.addedNodes.length) {
                        needApply = true;
                    }
                });
                if (needApply) applyMoveLockUI();
            });
            mo.observe($main.get(0), { childList: true, subtree: true });
            // 保存以便需要時關閉
            self._moveLockObserver = mo;
        }
    }
    iconPicker.on('change', function (e) {
        $form.find("[name=icon]").val(e.icon);
    });
    $main.on('click', '.btnRemove', function (e) {
        e.preventDefault();
        var btn = this;
        var $li = $(btn).closest('li');
        var data = $li.data();
        var title = data.text;
        co.sweet.confirm("即將刪除", settings.textConfirmDelete.replace("{0}", title), "確認", "取消", function () {
            var list = $(btn).closest('ul');
            $li.remove();
            var isMainContainer = false;
            if (typeof list.attr('id') !== 'undefined') {
                isMainContainer = (list.attr('id').toString() === idSelector);
            }
            if ((!list.children().length) && (!isMainContainer)) {
                list.prev('div').children('.sortableListsOpener').first().remove();
                list.remove();
            }
            if (itemEditing && itemEditing.length && itemEditing.is($li)) {
                resetForm();
            }
            !!settings.on.del && settings.on.del(data);
            self.updateButtons($main);
        });
    });
    $main.on("click", "li.list-group-item", function (e) {
        if ($(e.target).closest(".btn-group").length > 0) {
            return;
        }
        e.preventDefault();
        e.stopPropagation();
        const $li = $main.find("li.selectItem").first();
        $main.find("li.selectItem").removeClass("selectItem");
        if ($li.data("id") != $(this).data("id"))
            $(this).addClass("selectItem");
        
        typeof settings.on.updateMenuEditorAddTitle === "function" && settings.on.updateMenuEditorAddTitle();
    });

    $main.on('click', '.btnPage', function (e) {
        e.preventDefault();
        itemEditing = $(this).closest('li');
        !!settings.on.page && settings.on.page($(itemEditing).data());
        editItem(itemEditing);
    });
    $main.on('click', '.btnPower', function (e) {
        e.preventDefault();
        itemEditing = $(this).closest('li');
        settings.on.setPower($(itemEditing).data());
        editItem(itemEditing);
    });
    $main.on('click', '.btnFrontPower', function (e) {
        e.preventDefault();
        itemEditing = $(this).closest('li');
        if (typeof settings.on.setFrontPower === "function") {
            settings.on.setFrontPower($(itemEditing).data(), itemEditing);
        }
    });
    $main.on('click', '.btnEdit', function (e) {
        e.preventDefault();
        itemEditing = $(this).closest('li');
        editItem(itemEditing);
        !!settings.on.edit && settings.on.edit();
    });

    $main.on('click', '.btnUp', function (e) {
        e.preventDefault();
        var $li = $(this).closest('li');
        $li.prev('li').before($li);
        self.updateButtons($main);
        !!settings.on.drop && settings.on.drop($li);
    });
    $main.on('click', '.btnDown', function (e) {
        e.preventDefault();
        var $li = $(this).closest('li');
        $li.next('li').after($li);
        self.updateButtons($main);
        !!settings.on.drop && settings.on.drop($li);
    });
    $main.on('click', '.btnOut', function (e) {
        e.preventDefault();
        var list = $(this).closest('ul');
        var $li = $(this).closest('li');
        var $liParent = $li.closest('ul').closest('li');
        $liParent.after($li);
        if (list.children().length <= 0) {
            list.prev('div').children('.sortableListsOpener').first().remove();
            list.remove();
        }
        self.updateButtons($main);
        $main.updateLevels();
        !!settings.on.drop && settings.on.drop($li);
    });
    $main.on('click', '.btnIn', function (e) {
        e.preventDefault();
        var $li = $(this).closest('li');
        var $prev = $li.prev('li');
        if (! isValidLevel($li, $prev)) {
            return false;
        }
        if ($prev.length > 0) {
            var $ul = $prev.children('ul');
            if ($ul.length > 0)
                $ul.append($li);
            else {
                var $ul = $('<ul>').addClass('pl-0').css('padding-top', '10px');
                $prev.append($ul);
                $ul.append($li);
                $prev.addClass('sortableListsOpen');
                TOpener($prev);
            }
        }
        self.updateButtons($main);
        $main.updateLevels();
        !!settings.on.drop && settings.on.drop($li);
    });

    /* PRIVATE METHODS */
    function editItem($item) {
        var data = $item.data();
        const $card = $form.parents(".card");
        self._editingDomId = _ensureDomId($item);
        $main.find("li.editItem").removeClass("editItem");
        $item.addClass("editItem");
        $.each(data, function (p, v) {
            let element = $form.find("[name=" + p + "]");
            if (element.length <= 0) return;
            switch (element[0].tagName) {
                case "SELECT":
                    element.find("[value=" + v + "]").prop("selected", true);
                    $(element).trigger("change");
                    break;
                default:
                    switch (element.attr("type").toUpperCase()) {
                        case "RADIO":
                            element.find("[value=" + v + "]").prop("checked", true);
                            break;
                        default:
                            element.val(v);
                            break;
                    }
                    break;
            }
        });
        if (data.linkUrl != null) {
            $card.find(".card-header>.title").text(data.text);
            $card.find(".card-header>a")
                .attr({
                    "href": data.linkUrl == "" ? `${defaultUrl}/${OrgName}/${data.routerName}`.replace(`${defaultUrl}//`, `${defaultUrl}/`) : data.linkUrl
                }).removeClass("d-none");
        }
        $form.find(".item-menu").first().focus();
        if (data.hasOwnProperty('icon')) {
            iconPicker.iconpicker('setIcon', data.icon);
        } else{
            iconPicker.iconpicker('setIcon', 'empty');
        }
        $updateButton.removeAttr('disabled');
    }

    function resetForm() {
        $form[0].reset();
        iconPicker = iconPicker.iconpicker(iconPickerOpt);
        iconPicker.iconpicker('setIcon', 'empty');
        $updateButton.attr('disabled', true);
        itemEditing = null;
    }

    function stringToArray(str) {
        try {
            var obj = JSON.parse(str);
        } catch (err) {
            console.log('The string is not a json valid.');
            return null;
        }
        return obj;
    }

    function TButton(attr) {
        return $("<a>").addClass(attr.classCss).addClass('clickable').attr("href", "#").html(attr.text);
    }

    function TButtonGroup() {
        var $divbtn = $('<div>').addClass('btn-group float-right');
        var $btnEdit = TButton({classCss: 'btn btn-primary btn-sm btnEdit', text: settings.labelEdit});
        var $btnRemv = TButton({classCss: 'btn btn-danger btn-sm btnRemove', text: settings.labelRemove});
        var $btnUp = TButton({classCss: 'btn btn-secondary btn-sm btnUp btnMove', text: '<i class="fas fa-angle-up clickable"></i>'});
        var $btnDown = TButton({classCss: 'btn btn-secondary btn-sm btnDown btnMove', text: '<i class="fas fa-angle-down clickable"></i>'});
        var $btnOut = TButton({ classCss: 'btn btn-secondary btn-sm btnOut btnMove levelMove', text: '<i class="fas fa-level-down-alt clickable"></i>'});
        var $btnIn = TButton({ classCss: 'btn btn-secondary btn-sm btnIn btnMove levelMove', text: '<i class="fas fa-level-up-alt clickable"></i>' });
        var $btnCont = TButton({ classCss: 'btn btn-success btn-sm btnPage', text: '<i class="fa fa-paint-roller clickable"></i>' });
        $divbtn.append($btnUp).append($btnDown).append($btnIn).append($btnOut);
        $divbtn.append($btnEdit).append($btnRemv).append($btnCont);
        return $divbtn;
    }
    function renderExtraButtons($title, $btnGroup, itemData, $li) {
        if (!settings.btn || !settings.btn.length) return;

        settings.btn.forEach(function (cfg) {
            if (typeof cfg.permission !== 'undefined' && !cfg.permission) return;
            if (typeof cfg.render !== 'function') return;

            var pos = (cfg.position || 'action').toLowerCase();
            var ctx = { data: itemData, $li: $li, editor: self };

            // 1) 交給外部生成按鈕（jQuery element）
            var $button = cfg.render(ctx);
            if (!$button || !$button.length) return;

            // 2) 綁定 click（若有）
            if (typeof cfg.click === 'function') {
                $button.on('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    cfg.click({
                        event: e,
                        $button: $button,
                        $li: $li,
                        data: $li.data(), // 每次點擊拿當前 data
                        editor: self
                    });
                });
            }

            // 3) 初始 init（若有）
            if (typeof cfg.init === 'function') {
                cfg.init({
                    $button: $button,
                    data: itemData,
                    $li: $li,
                    editor: self
                });
            }

            // 4) 根據 position 決定插在「哪一段」
            if (pos === 'title') {
                // → 標題後面
                $title.append($button);
            } else if (pos === 'move') {
                // → 排序群後面（最後一顆 .btnMove 的後面）
                var $lastMove = $btnGroup.find('.btnMove').last();
                if ($lastMove.length) {
                    $button.insertAfter($lastMove);
                } else {
                    // 找不到就退而求其次，塞到開頭
                    $btnGroup.prepend($button);
                }
            } else { // 'action' 或其他
                // → 操作群前面（第一顆 .btnEdit 前面）
                var $firstAction = $btnGroup.find('.btnEdit').first();
                if ($firstAction.length) {
                    $button.insertBefore($firstAction);
                } else {
                    // 找不到就 append 在整組最後
                    $btnGroup.append($button);
                }
            }
        });
    }

    /**
     * @param {array} arrayItem Object Array
     * @param {int} depth Depth sub-menu
     * @return {object} jQuery Object
     **/
    function createMenu(arrayItem, depth) {
        var level = (typeof depth === 'undefined') ? 0 : depth;
        var $elem = (level === 0)
            ? $main
            : $('<ul>').addClass('pl-0').css('padding-top', '10px').data("level", level);

        $.each(arrayItem, function (k, v) {
            var isParent = Array.isArray(v.children);
            var itemObject = { text: "", href: "", icon: "empty", target: "_self", title: "" };
            var temp = $.extend({}, v);

            if (isParent) delete temp.children;
            $.extend(itemObject, temp);
            var $li = _buildLi(itemObject, level);
            if (isParent) {
                var $childUl = createMenu(v.children, level + 1);
                $li.append($childUl);
            }

            $elem.append($li);
        });

        return $elem;
    }
    function _buildLi(itemObject) {
        var $li = $('<li>').addClass('list-group-item pr-0');
        $li.data(itemObject);

        var $div = $('<div>').css('overflow', 'auto');

        var $i = $('<i>').addClass(itemObject.icon);
        if ($i.hasClass("material-symbols-outlined")) {
            $i.text(itemObject.icon.replace("material-symbols-outlined", "").trim());
        }

        var $span = $('<span>')
            .addClass('txt font-weight-bold')
            .text(itemObject.text)
            .css('margin-right', '5px');

        var $divTitle = $("<div class='d-flex align-items-center float-left' />");
        $divTitle.append($i).append("&nbsp;").append($span);

        var $divbtn = TButtonGroup();

        // ← 保留你現有的擴充按鈕（不做任何改動）
        renderExtraButtons($divTitle, $divbtn, itemObject, $li);
        $div.append($divTitle).append($divbtn);
        $li.append($div);

        return $li;
    }


    function TOpener(li){
        var opener = $('<span>').addClass('sortableListsOpener ' + options.opener.openerClass).css(options.opener.openerCss)
                .on('mousedown touchstart', function (e){
                    var li = $(this).closest('li');
                    if (li.hasClass('sortableListsClosed')) {
                        li.iconOpen(options);
                    } else {
                        li.iconClose(options);
                    }
                    return false; // Prevent default
                });
        opener.prependTo(li.find('div.float-left').first());
        if ( !li.hasClass('sortableListsOpen') ) {
            li.iconClose(options);
        } else {
            li.iconOpen(options);
        }
    }

    function setOpeners() {
        $main.find('li').each(function () {
            var $li = $(this);
            if ($li.children('ul').length) {
                TOpener($li);
            }
        });
    }

    function isValidLevel($li, $liTarget) {
        if (settings.maxLevel < 0){
            return true;
        }
        var targetLevel = 0;
        var liCount = $li.find('ul').length;
        if ($liTarget.length==0) {
            targetLevel = 0;
        } else {
            targetLevel = parseInt($liTarget.parent().data("level")) + 1;
        }
        var newLevel = (targetLevel + liCount);
        if ($li.data("minLevel") > newLevel || ($li.data("maxLevel") > 0 && $li.data("maxLevel") < newLevel)) return false
        else return (newLevel <=settings.maxLevel)
    }

    /* PUBLIC METHODS */
    self.TOpener = TOpener;
    self.setForm = function(form){
        $form = form;
    };

    self.getForm = function(){
        return $form;
    };

    self.setUpdateButton = function($btn) {
        $updateButton = $btn;
        $updateButton.attr('disabled', true);
        itemEditing = null;
    };

    self.getUpdateButton = function(){
        return $updateButton;
    };

    self.getCurrentItem = function(){
        return itemEditing;
    };

    self.update = function () {
        var $cEl = self.getCurrentItem();
        if ($cEl===null){
            return;
        }
        if (!$form[0].checkValidity()) {
            $form.addClass("was-validated");
        } else {
            var oldIcon = $cEl.data('icon');
            $form.find('.item-menu').each(function () {
                $cEl.data($(this).attr('name'), $(this).val());
            });
            $cEl.data('text', $cEl.data('title'));
            $cEl.children().children('i').removeClass(oldIcon).addClass($cEl.data('icon'));
            $cEl.find('span.txt').first().text($cEl.data('text'));
            !!settings.on.update && settings.on.update($cEl.data());
        }
        //resetForm();
    };
   
    self.add = function () {
        if (!$form[0].checkValidity()) {
            $form.addClass("was-validated");
            return;
        }
        var data = {};
        $form.find('.item-menu').each(function () {
            data[$(this).attr('name')] = $(this).val();
        });

        if (!data.Id) data.Id = 0;
        if (!data.SerNO) data.SerNO = 500;
        data.text = data.title;
        var $li = _buildLi(data);
        $main.append($li);
        self.updateButtons($main);
        !!settings.on.add && settings.on.add($li);
    };


    self.refresh = function () {
        resetForm();
    }
    /**
     * Data Output
     * @return String JSON menu scheme
     */
    self.getString = function () {
        var obj = $main.sortableListsToJson();
        return JSON.stringify(obj);
    };
    /**
     * Data Input
     * @param {Array} Object array. The nested menu scheme
     */
    self.setData = function (strJson) {
        var arrayItem = (Array.isArray(strJson)) ? strJson : stringToArray(strJson);
        if (arrayItem !== null) {
            $main.empty();
            var menu = createMenu(arrayItem);
            if (!sortableReady) {
                menu.sortableLists(settings.listOptions);
                sortableReady = true;
            } else {
                setOpeners();
            }
            self.updateButtons($main);
        }
    };

    // === setDataPreserve：沿用原本 setData，只做狀態保存/還原（Id優先，索引路徑為後援） ===
    (function patchSetDataPreserve() {
        var originalSetData; // 第一次呼叫時才抓，避免未定義
        var norm = function (v) { return (v == null ? "" : String(v)); };

        // 取得 li 的「索引路徑」(0-based)：[rootIndex, childIndex, ...]
        function getPath($li) {
            var path = [];
            var $cur = $li;
            while ($cur && $cur.length && !$cur.is($li.closest('#__ROOT_STOP__'))) {
                var $ul = $cur.parent('ul');
                var idx = $cur.prevAll('li').length;
                path.unshift(idx);
                // 往上找到父層 li
                $cur = $ul.parent('li');
                if (!$cur.length) break;
            }
            return path;
        }

        // 依「索引路徑」在新 DOM 找到 li
        function findByPath(rootUl, path) {
            var $curUl = rootUl;
            var $li = null;
            for (var i = 0; i < path.length; i++) {
                $li = $curUl.children('li').eq(path[i]);
                if (!$li.length) return null;
                $curUl = $li.children('ul');
                if (i < path.length - 1 && !$curUl.length) return null;
            }
            return $li || null;
        }

        // 嘗試從 li 讀出 Id（支援 Id/id）
        function readId($li) {
            // jQuery .data 會做 camelCase 正規化，這裡兩個都試
            return norm($li.data('Id')) || norm($li.data('id')) || "";
        }

        self.setDataPreserve = function (strJson) {
            // 延後擷取核心 setData
            if (typeof originalSetData !== 'function') originalSetData = self.setData;
            var coreSet = (typeof originalSetData === 'function') ? originalSetData : self.setData;
            if (typeof coreSet !== 'function' || coreSet === self.setDataPreserve) {
                console.error('MenuEditor: setData not ready when setDataPreserve called.');
                return;
            }

            // 1) 保存狀態：展開 li 的 Id 與 索引路徑，還有卷軸與鎖定
            var openStates = [];
            $main.find('li.sortableListsOpen').each(function () {
                var $li = $(this);
                openStates.push({
                    id: readId($li),         // 可能為空
                    path: getPath($li)       // 一定有
                });
            });
            const editorId = $(itemEditing).attr("id");
            var prevScroll = $main.scrollTop();
            var wasLocked = !!self._moveLocked;

            // 2) 重建（沿用原本 setData）
            coreSet.call(self, strJson);
            _tagAllDomIds(); 
            if (self._editingDomId) {
                var $new = $('#' + _cssEscape(self._editingDomId), $main);
                if ($new.length) {
                    itemEditing = $new.addClass('editItem');
                    editItem(itemEditing); // 讓右側表單拿到最新 data
                }
            }

            // 3) 還原展開狀態（先用 Id 找，找不到再用索引路徑）
            //    注意：createMenu後 setOpeners() 會預設關閉，所以我們要在它之後打開
            //    原本 setData 已經呼叫過 setOpeners()，這裡直接打開即可
            for (var i = 0; i < openStates.length; i++) {
                var st = openStates[i], target = null;

                if (st.id) {
                    // 用 Id/id 對應
                    target = $main.find('li').filter(function () {
                        var d = readId($(this));
                        return d && d === st.id;
                    }).first();
                }
                if (!target || !target.length) {
                    // 後援：用索引路徑
                    target = findByPath($main, st.path);
                }
                if (target && target.length) {
                    target.iconOpen(settings.listOptions);
                }
            }

            // 4) 還原卷軸與鎖定＋鎖定按鈕圖示
            $main.scrollTop(prevScroll);
            if (typeof self.toggleMoveLock === 'function') self.toggleMoveLock(wasLocked);
            if (typeof self._renderMoveLockBtn === 'function') self._renderMoveLockBtn(wasLocked);
        };
    })();

    if (!!settings.element) {
        var e = settings.element;
        self.setForm($(e.Form));
        self.setUpdateButton($(e.Update));
        //Calling the update method
        $(e.Update).click(self.update);
        // Calling the add method
        $(e.Add).click(self.add);
        $(e.Refresh).click(self.refresh);
    }
    !!settings.on.ready && settings.on.ready();
};
