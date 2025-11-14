package backend.cowrite.publisher;

import backend.cowrite.common.event.EventType;
import backend.cowrite.common.event.payload.MoveEventPayload;
import backend.cowrite.common.outboxmessagerelay.pub.OutboxEventPublisher;
import lombok.RequiredArgsConstructor;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

@Component
@RequiredArgsConstructor
public class MoveUpdatePublisher {
    private final OutboxEventPublisher outboxEventPublisher;

    @Transactional
    public void updateDocument(Long roomId, MoveEventPayload eventPayloadPayload) {
        outboxEventPublisher.publish(EventType.INPUT, eventPayloadPayload, roomId);
    }
}
