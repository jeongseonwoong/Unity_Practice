package backend.cowrite.consumer;

import backend.cowrite.common.event.Event;
import backend.cowrite.common.event.EventPayload;
import backend.cowrite.common.event.EventType;
import backend.cowrite.service.MoveUpdateService;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.springframework.kafka.annotation.KafkaListener;
import org.springframework.kafka.support.Acknowledgment;
import org.springframework.messaging.simp.SimpMessagingTemplate;
import org.springframework.stereotype.Component;

import static backend.cowrite.config.WebsocketConfig.ClientSubscribeRoute;

@Component
@Slf4j
@RequiredArgsConstructor
public class MoveUpdateEventConsumer {
    private final SimpMessagingTemplate messagingTemplate;


    @KafkaListener(
            topics = {EventType.Topic.UPDATE},
            containerFactory = "kafkaListenerContainerFactory"
    )
    public void listen(ConsumerRecord<String, String> outbox, Acknowledgment ack) {
        String outboxRoomId = outbox.key();
        String outboxEvent = outbox.value();
        log.info("[MoveUpdateEventConsumer.listen()] roomId = {}, message = {}", outboxRoomId, outboxEvent);

        Event<EventPayload> event = Event.fromJson(outboxEvent);
        if (event != null) {
            String destination = clientSubscribeRoute(outboxRoomId);
            messagingTemplate.convertAndSend(destination, outboxEvent);
        }
        ack.acknowledge();
    }

    private String clientSubscribeRoute(String key) {
        return String.format("%s/%s", ClientSubscribeRoute, key);
    }

}
