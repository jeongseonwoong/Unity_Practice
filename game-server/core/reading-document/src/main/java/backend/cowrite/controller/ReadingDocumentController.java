package backend.cowrite.controller;

import backend.cowrite.common.event.payload.MoveEventPayload;
import backend.cowrite.publisher.MoveUpdatePublisher;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.messaging.handler.annotation.DestinationVariable;
import org.springframework.messaging.handler.annotation.MessageMapping;
import org.springframework.messaging.handler.annotation.Payload;
import org.springframework.stereotype.Controller;

@Controller
@RequiredArgsConstructor
@Slf4j
public class ReadingDocumentController {

    private final MoveUpdatePublisher documentUpdatePublisher;

    @MessageMapping("/{documentId}")
    public void editDocument(@DestinationVariable Long documentId, @Payload MoveEventPayload moveEventPayload) {
        documentUpdatePublisher.updateDocument(documentId, moveEventPayload);
    }

}
