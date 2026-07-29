(function (w) {
    "use strict";

    var Coker = (w.Coker = w.Coker || {});
    var initialized = false;

    function createEventId() {
        if (w.crypto && typeof w.crypto.randomUUID === "function") {
            return w.crypto.randomUUID();
        }

        return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (char) {
            var random = Math.random() * 16 | 0;
            var value = char === "x" ? random : (random & 0x3 | 0x8);
            return value.toString(16);
        });
    }

    function init() {
        if (initialized) return;

        var token = w.RemoteTrackingToken;
        if (!token || !Coker.RemoteTracking || typeof Coker.RemoteTracking.Collect !== "function") {
            return;
        }

        initialized = true;

        var eventId = createEventId();
        var visibleMilliseconds = 0;
        var visibleStartedAt = document.hidden ? null : performance.now();
        var hasInteraction = false;
        var requestInFlight = false;
        var pendingCollect = false;
        var milestoneTimer = null;
        var milestones = [2, 10];
        var nextMilestoneIndex = 0;

        function getVisibleSeconds() {
            if (visibleStartedAt !== null) {
                var now = performance.now();
                visibleMilliseconds += now - visibleStartedAt;
                visibleStartedAt = now;
            }

            return Math.min(300, Math.floor(visibleMilliseconds / 1000));
        }

        function createPayload() {
            return {
                Token: token,
                EventId: eventId,
                VisibleSeconds: getVisibleSeconds(),
                HasInteraction: hasInteraction
            };
        }

        function syncMilestones(visibleSeconds) {
            var advanced = false;

            while (nextMilestoneIndex < milestones.length
                && visibleSeconds >= milestones[nextMilestoneIndex]) {
                nextMilestoneIndex++;
                advanced = true;
            }

            if (advanced && milestoneTimer !== null) {
                w.clearTimeout(milestoneTimer);
                milestoneTimer = null;
            }
        }

        function scheduleNextMilestone() {
            if (milestoneTimer !== null) {
                w.clearTimeout(milestoneTimer);
                milestoneTimer = null;
            }

            if (document.hidden || nextMilestoneIndex >= milestones.length) return;

            var targetMilliseconds = milestones[nextMilestoneIndex] * 1000;
            var remainingMilliseconds = Math.max(0, targetMilliseconds - visibleMilliseconds);

            milestoneTimer = w.setTimeout(function () {
                milestoneTimer = null;
                collect(false);
                scheduleNextMilestone();
            }, remainingMilliseconds);
        }

        function collect(useBeacon) {
            var payload = createPayload();
            syncMilestones(payload.VisibleSeconds);
            if (payload.VisibleSeconds < 2) return;

            if (useBeacon) {
                Coker.RemoteTracking.Collect(payload, true);
                return;
            }

            if (requestInFlight) {
                pendingCollect = true;
                return;
            }

            requestInFlight = true;
            Coker.RemoteTracking.Collect(payload, false).always(function () {
                requestInFlight = false;
                if (pendingCollect) {
                    pendingCollect = false;
                    collect(false);
                }
            });
        }

        function markInteraction() {
            if (hasInteraction) return;

            hasInteraction = true;
            collect(false);
            scheduleNextMilestone();
        }

        document.addEventListener("pointerdown", markInteraction, { once: true, passive: true });
        document.addEventListener("keydown", markInteraction, { once: true });
        document.addEventListener("scroll", markInteraction, { once: true, passive: true });

        scheduleNextMilestone();

        w.setInterval(function () {
            if (!document.hidden) collect(false);
        }, 60000);

        document.addEventListener("visibilitychange", function () {
            if (document.hidden) {
                collect(true);
                visibleStartedAt = null;
                if (milestoneTimer !== null) {
                    w.clearTimeout(milestoneTimer);
                    milestoneTimer = null;
                }
            } else {
                visibleStartedAt = performance.now();
                scheduleNextMilestone();
            }
        });

        w.addEventListener("pagehide", function () {
            collect(true);
        });
    }

    Coker.extend({
        RemoteTracking: {
            init: init
        }
    });
})(window);
