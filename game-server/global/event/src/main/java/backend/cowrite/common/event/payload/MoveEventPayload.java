package backend.cowrite.common.event.payload;

import backend.cowrite.common.event.EventPayload;
import lombok.*;

@Getter
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class MoveEventPayload implements EventPayload {
    Operation operation;
}
