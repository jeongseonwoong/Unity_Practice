package backend.cowrite.common.event;

import backend.cowrite.common.event.payload.MoveEventPayload;
import lombok.Getter;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;

@Slf4j
@Getter
@RequiredArgsConstructor
public enum EventType {

    INPUT(MoveEventPayload.class, Topic.UPDATE);

    private final Class<? extends EventPayload> payloadClass;
    private final String topic;

    public static class Topic {
        public static final String UPDATE = "user-typing";
    }
}
